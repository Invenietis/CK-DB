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
    if @UserId = 0 throw 50000, 'Argument.InvalidValue', 1;

	if @PwdHash is null set @PwdHash = 0x0;

	--[beginsp]

	--<PreCreateUserPassword reverse /> 

	insert into CK.tUserPassword(UserId, PwdHash, LastWriteTime ) values( @UserId, @PwdHash, sysutcdatetime());

	--<PostCreateUserPassword /> 

	--[endsp]
end