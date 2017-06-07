-- SetupConfig: {}
--
-- @SchemeSuffix: Scheme suffix to delete.
--                When null, all registrations for this provider regardless 
--                of the scheme suffix are deleted.
--
create procedure CK.sUserOidcDestroy
(
	@ActorId int,
	@UserId int,
    @SchemeSuffix varchar(64)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId = 0 throw 50000, 'Argument.InvalidValue', 1;

	--[beginsp]

	--<PreDestroy revert /> 
	
    if @SchemeSuffix is null
    begin
	    delete CK.tUserOidc where UserId = @UserId;
    end
    else
    begin
	    delete CK.tUserOidc where UserId = @UserId and SchemeSuffix = @SchemeSuffix;
    end
	--<PostDestroy /> 

	--[endsp]
end