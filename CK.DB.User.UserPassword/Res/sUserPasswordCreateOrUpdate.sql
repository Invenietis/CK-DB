-- SetupConfig: {}
--
-- @Mode: CreateOnly = 1, UpdateOnly = 2, CreateOrUpdate = 3, WithLogin = 4
-- @Result: None = 0, Created = 1, Updated = 2
--
create procedure CK.sUserPasswordCreateOrUpdate
(
	@ActorId int,
	@UserId int /*input*/output, 
	@PwdHash varbinary(64),
	@Mode int, -- not null enum { "CreateOnly" = 1, "UpdateOnly" = 2, "CreateOrUpdate" = 3, "WithLogin" = 4, "IgnoreOptimisticKey" = 8 }
	@Result int output, -- not null enum { None = 0, Created = 1, Updated = 2 }
    @FailureCode int output, -- Optional. Set by CK.sAuthUserOnLogin if login is rejected.
    @FailureReason varchar(128) output -- Optional. Set by CK.sAuthUserOnLogin if login is rejected.
)
as
begin
	-- Handles @Mode: extracts @ActualLogin bit for readbility.
	declare @ActualLogin bit = 0;
	if (@Mode&4) <> 0 
	begin
		set @ActualLogin = 1;
		set @Mode = @Mode&~4;
	end
	-- Clears IgnoreOptimisticKey since we do not use it here.
	set @Mode = (@Mode&~8);
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId <= 0 throw 50000, 'Argument.InvalidUserId', 1;
    if @Mode is null or @Mode < 1 or @Mode > 3 throw 50000, 'Argument.InvalidMode', 1;
	if @PwdHash is null  or DataLength(@PwdHash) = 0 throw 50000, 'Argument.InvalidUserPwdHash', 1;

	--[beginsp]
	declare @LastLoginTime datetime2(2);
	declare @Now datetime2(2) = sysutcdatetime();
	select @LastLoginTime = LastLoginTime from CK.tUserPassword where UserId = @UserId;

	if @LastLoginTime is null
	begin
		if (@Mode&1) <> 0
		begin
			if @ActualLogin = 1 set @LastLoginTime = @Now;
			else set @LastLoginTime ='0001-01-01';
			--<PreCreate revert /> 

			insert into CK.tUserPassword( UserId, PwdHash, LastWriteTime, LastLoginTime ) values( @UserId, @PwdHash, @Now, @LastLoginTime);
			set @Result = 1;

			--<PostCreate /> 
		end
		else set @Result = 0;
	end
	else
	begin
		if (@Mode&2) <> 0
		begin
			if @ActualLogin = 1 set @LastLoginTime = @Now;

			--<PreUpdate revert /> 
			update CK.tUserPassword set PwdHash = @PwdHash, LastWriteTime = @Now, LastLoginTime = @LastLoginTime where UserId = @UserId;
			set @Result = 2;
			--<PostUpdate /> 
		end
		else set @Result = 0;
	end

    if @Result <> 0 and @ActualLogin = 1
	begin
		exec CK.sAuthUserOnLogin 'Basic', @LastLoginTime, @UserId, @FailureCode output, @FailureReason output;  
	end

	--[endsp]
end
