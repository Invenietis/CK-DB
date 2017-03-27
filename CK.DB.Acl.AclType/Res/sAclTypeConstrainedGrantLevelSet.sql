-- SetupConfig: { }
create procedure CK.sAclTypeConstrainedGrantLevelSet 
(
	@ActorId int,
	@AclTypeId int,
	@Set bit
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	--<PreAclTypeConstrainedGrantLevel revert />
	update CK.tAclType set ConstrainedGrantLevel = @Set 
		where AclTypeId = @AclTypeId;
	if @@RowCount > 0 and @Set = 1
	begin
		-- Check that defined levels actually match all existing configurations.
		if exists( select *
					from CK.tAclType t
					inner join CK.tAcl a on a.AclTypeId = t.AclTypeId
					inner join CK.tAclConfigMemory m on m.AclId = a.AclId
					where t.AclTypeId = @AclTypeId 
							and (case when m.GrantLevel >= 127 then 255 - m.GrantLevel else m.GrantLevel end) 
									not in (select l.GrantLevel from CK.tAclTypeGrantLevel l where l.AclTypeId = @AclTypeId) )
		begin
			;throw 50000, 'Security.AclTypeConstrainedLevelMismatch', 1;
		end
	end
	--<PostAclTypeConstrainedGrantLevel />

	--[endsp]
end

