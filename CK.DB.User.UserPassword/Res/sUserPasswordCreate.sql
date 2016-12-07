-- SetupConfig: {}
--
create procedure CK.sUserPasswordCreate
(
	@ActorId int,
	@UserId int, 
	@PwdHash varbinary(64)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId <= 0 throw 50000, 'Argument.InvalidUserId', 1;
	if @PwdHash is null  or DataLength(@PwdHash) = 0 throw 50000, 'Argument.InvalidUserPwdHash', 1;

	--[beginsp]

	--<PreCreate reverse /> 

	insert into CK.tUserPassword(UserId, PwdHash, LastWriteTime ) values( @UserId, @PwdHash, sysutcdatetime());

	--<PostCreate /> 

	--[endsp]
end