-- Version = *
alter Procedure CK.sAclCreate
(
    @ActorId int,
    @AclIdResult int output
)
as begin

--[beginsp]
	insert into CK.tAcl default values;
	set @AclIdResult = scope_identity();
--[endsp]
end
