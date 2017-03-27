-- SetupConfig: {}
alter Procedure CK.sAclCreate
(
    @ActorId int,
    @AclIdResult int output
)
as begin

	--[beginsp]

	--<PreCreate revert />

	insert into CK.tAcl default values;
	set @AclIdResult = scope_identity();

	--<PostCreate />

	--[endsp]
end
