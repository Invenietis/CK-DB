-- SetupConfig: { }
create procedure CK.sAclTypeGrantLevelSet 
(
	@ActorId int,
	@AclTypeId int,
	@GrantLevel tinyint,
	@Set bit
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GrantLevel > 127 throw 50000, 'Security.GrantLevelMustBeAtMost127', 1;
	if @Set = 0 and (@GrantLevel = 0 or @GrantLevel = 127) throw 50000, 'Security.GrantLevel0And127CanNotBeRemoved', 1;

	--[beginsp]

	if @Set = 0
	begin
		declare @GrantLevelDeny tinyint = 255 - @GrantLevel;
		--<PreRemoveLevel revert />
		-- When removing a level, we must ensure that, if the AclType is constrained, the 
		-- removed level does not appear in any Acl current configuration.
		if exists( select *
					from CK.tAcl a
					inner join CK.tAclType t on t.AclTypeId = a.AclTypeId
					inner join CK.tAclConfigMemory m on m.AclId = a.AclId
					where a.AclTypeId = @AclTypeId 
							and t.ConstrainedGrantLevel = 1
							and (m.GrantLevel = @GrantLevel or m.GrantLevel = @GrantLevelDeny))
		begin
			;throw 50000, 'Security.AclTypeConstrainedLevelIsUsed', 1;
		end
		delete l from CK.tAclTypeGrantLevel l where l.AclTypeId = @AclTypeId and l.GrantLevel = @GrantLevel;
		--<PostRemoveLevel />
	end
	else 
	begin
		--<PreAddLevel revert />
		-- One can always add a level.
		merge CK.tAclTypeGrantLevel as target
			using ( select AclTypeId = @AclTypeId, GrantLevel = @GrantLevel ) 
			as source on source.AclTypeId = target.AclTypeId and source.GrantLevel = target.GrantLevel
			when not matched by target then insert ( AclTypeId, GrantLevel ) values( source.AclTypeId, source.GrantLevel );
		--<PostAddLevel />
	end

	--[endsp]
end

