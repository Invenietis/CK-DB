-- SetupConfig: { }
create procedure CK.sAclTypeDestroy
(
	@ActorId int,
	@AclTypeId int
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	--<PreAclTypeDestroy revert />

	delete from CK.tAclTypeGrantLevel where AclTypeId = @AclTypeId;
	delete from CK.tAclType where AclTypeId = @AclTypeId;
			
	--<PostAclTypeDestroy />

	--[endsp]
end

