-- SetupConfig: {}
--
create procedure CK.sUserPasswordPwdHashSet
(
	@ActorId int,
	@UserId int, 
	@PwdHash varbinary(64)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId = 0 throw 50000, 'Argument.InvalidUserId', 1;
	if @PwdHash is null  or DataLength(@PwdHash) = 0 throw 50000, 'Argument.InvalidUserPwdHash', 1;

	--[beginsp]

	--<PreSetPwdHash reverse /> 

	update CK.tUserPassword set PwdHash = @PwdHash, LastWriteTime = sysutcdatetime() where UserId = @UserId;

	--<PostSetPwdHash /> 

	--[endsp]
end