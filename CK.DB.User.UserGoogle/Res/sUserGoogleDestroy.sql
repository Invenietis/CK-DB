-- SetupConfig: {}
--
create procedure CK.sUserGoogleDestroy
(
	@ActorId int,
	@UserId int
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId = 0 throw 50000, 'Argument.InvalidValue', 1;

	--[beginsp]
	declare @ScopeSetId int;
	select @ScopeSetId = ScopeSetId from CK.tUserGoogle where UserId = @UserId;

	--<PreDestroy reverse /> 
	
	delete CK.tUserGoogle where UserId = @UserId;
	exec CK.sAuthScopeSetDestroy @ActorId, @ScopeSetId;

	--<PostDestroy /> 

	--[endsp]
end