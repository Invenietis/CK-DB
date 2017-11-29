-- SetupConfig: {}
--
-- @Mode: CreateOnly = 1, UpdateOnly = 2, CreateOrUpdate = 3, WithCheckLogin = 4, WithActualLogin = 8
-- @UCResult: None = 0, Created = 1, Updated = 2
--
create procedure CK.sUserPasswordUCL
(
	@ActorId int,
	@UserId int /*input*/output, 
	@PwdHash varbinary(64),
	@Mode int, -- not null enum { "CreateOnly" = 1, "UpdateOnly" = 2, "CreateOrUpdate" = 3, "WithCheckLogin" = 4, "WithActualLogin" = 8, "IgnoreOptimisticKey" = 16 }
	@UCResult int output, -- not null enum { None = 0, Created = 1, Updated = 2 }
    @LoginFailureCode int /*input*/output,
    @LoginFailureReason nvarchar(255) output
)
as
begin
	-- Clears IgnoreOptimisticKey since we do not use it here.
	set @Mode = (@Mode&~16);
    if @Mode is null or @Mode < 1 or @Mode > 15 throw 50000, 'Argument.InvalidMode', 1;
	-- Handles @Mode: extracts @CheckLogin & @ActualLogin bit for readability.
	declare @CheckLogin bit = 0;
	declare @ActualLogin bit = 0;
	if (@Mode&8) <> 0 
	begin
		set @ActualLogin = 1;
        set @CheckLogin = 1;
		set @Mode = @Mode&~(4+8);
	end
    else if (@Mode&4) <> 0
    begin
        set @CheckLogin = 1;
		set @Mode = @Mode&~4;
    end
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId <= 0 throw 50000, 'Argument.InvalidUserId', 1;

	--[beginsp]
	declare @LastLoginTime datetime2(2);
	declare @Now datetime2(2) = sysutcdatetime();
	select @LastLoginTime = LastLoginTime from CK.tUserPassword where UserId = @UserId;

	if @LastLoginTime is null
	begin
		if (@Mode&1) <> 0
		begin
	        if @PwdHash is null or DataLength(@PwdHash) = 0 throw 50000, 'Argument.InvalidUserPwdHash', 1;
			set @LastLoginTime = '0001-01-01';
			--<PreCreate revert /> 
			insert into CK.tUserPassword( UserId, PwdHash, LastWriteTime, LastLoginTime )
                values( @UserId, @PwdHash, @Now, @LastLoginTime);
			set @UCResult = 1;
			--<PostCreate /> 
		end
		else set @UCResult = 0;
	end
	else
	begin
		if (@Mode&2) <> 0
		begin
	        if @PwdHash is not null and DataLength(@PwdHash) = 0 throw 50000, 'Argument.InvalidUserPwdHash', 1;
			--<PreUpdate revert /> 
			update CK.tUserPassword set
                PwdHash = case when @PwdHash is null then PwdHash else @PwdHash end,
                LastWriteTime = case when @PwdHash is null then @Now else LastWriteTime end
              where UserId = @UserId;
			set @UCResult = 2;
			--<PostUpdate />
		end
		else set @UCResult = 0;
	end

    if @CheckLogin = 1
	begin
        if @LastLoginTime is null set @LoginFailureCode = 2; -- UnregisteredUser
        else
        begin
		    exec CK.sAuthUserOnLogin 'Basic', @LastLoginTime, @UserId, @ActualLogin, @Now, @LoginFailureCode output, @LoginFailureReason output;
            if @ActualLogin = 1 and @LoginFailureCode is null
            begin
			    update CK.tUserPassword set LastLoginTime = @Now where UserId = @UserId;
            end
        end
	end
    else set @LoginFailureCode = 0; -- None

	--[endsp]
end
