-- SetupConfig: {}
ALTER Procedure CK.sAclGrantSet
(
    @ActorId int,
    @AclId int,
    @ActorIdToGrant int,
	@KeyReason varchar(128), -- A Key reason is optionnal. 
    @GrantLevel tinyint
)
as begin
	
	-- System is never granted since it has by design full control on all ACL.
	if @ActorIdToGrant = 1 return 0;

    -- Only the "System Acl" (1) is mutable. But it is protected... by itself.
    if @AclId = 1
    begin
        if CK.fAclGrantLevel( @ActorId, @AclId ) < 127 throw 50000, 'Security.SystemAclId.MustBe127OnItself', 1;
    end
    else
    begin
        if @AclId <= 8 throw 50000, 'Security.ImmutableAclId', 1;
    end

	if @KeyReason is null set @KeyReason = '';

	--[beginsp]



	--<PreMemoryUpdate revert />

	-- First, update CK.tAclConfigMemory with the new configuration given.
	merge CK.tAclConfigMemory as target
		using
		(
			select AclId = @AclId, ActorId = @ActorIdToGrant, KeyReason = @KeyReason
		) 
		as source on 
				source.AclId = target.AclId and 
				source.ActorId = target.ActorId and
				source.KeyReason = target.KeyReason
		when matched and @GrantLevel = 0 then delete -- When @GrantLevel = 0, we remove the entry.
		when matched and target.GrantLevel <> @GrantLevel then update set GrantLevel = @GrantLevel
		when not matched by target and @GrantLevel > 0 then insert( AclId, ActorId, KeyReason, GrantLevel ) values( @AclId, @ActorIdToGrant, @KeyReason, @GrantLevel ); -- When @GrantLevel <> 0, we insert.
	
	if @@ROWCOUNT > 0
	begin
		--<PreConfigUpdate revert />
		-- The memory drives the content of the AclConfig.
		merge CK.tAclConfig as target
			using
			(
				select AclId, ActorId, MaxGrantLevel = max(GrantLevel)
				from CK.tAclConfigMemory m
				where AclId = @AclId and ActorId = @ActorIdToGrant
				group by AclId, ActorId
			) 
			as source on 
				source.AclId = target.AclId and source.ActorId = target.ActorId 
			when not matched by target then insert( AclId, ActorId, GrantLevel ) values( @AclId, @ActorIdToGrant, source.MaxGrantLevel )
			when not matched by source and target.AclId = @AclId and target.ActorId = @ActorIdToGrant then delete 
			when matched and target.GrantLevel <> source.MaxGrantLevel then update set GrantLevel = source.MaxGrantLevel;		
		--<PostConfigUpdate />
	end

	--<PostMemoryUpdate />

	--[endsp]
end
