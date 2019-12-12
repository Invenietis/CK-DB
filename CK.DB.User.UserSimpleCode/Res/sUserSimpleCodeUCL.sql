-- SetupConfig: {}
--
-- @SimpleCode can not be null. This is the key that identifies the user.
--
-- @Mode (flags): CreateOnly = 1, UpdateOnly = 2, CreateOrUpdate = 3, WithCheckLogin = 4, WithActualLogin = 8.
--                @Mode is normalized:
--                  - WithActualLogin implies WithCheckLogin.
--
-- @UCResult: None = 0, Created = 1, Updated = 2
--
-- When @UserId = 0 we are in "login mode": 
--  - @Mode must be UpdateOnly+WithCheckLogin (6) or UpdateOnly+WithActualLogin (10).
--    If the SimpleCode is found, we update what we have to and output the found @UserId.
--
-- When @UserId is not 0, it must match with the one of the @SimpleCode otherwise it is an error
-- and an exception is thrown because:
--  - When updating it means that there is a mismatch of UserId/SimpleCode account in the calling code.
--  - When creating it means that another user with the same SimpleCode is already registered and
--    this should never happen.
--
-- When extending this procedure, during update null parameters must be left unchanged.
--
create procedure CK.sUserSimpleCodeUCL
(
	@ActorId int,
	@UserId int /*input*/output,
	@SimpleCode nvarchar(128), 
	@Mode int, -- not null enum { "CreateOnly" = 1, "UpdateOnly" = 2, "CreateOrUpdate" = 3, "WithCheckLogin" = 4, "WithActualLogin" = 8, "IgnoreOptimisticKey" = 16 }
	@UCResult int output, -- not null enum { None = 0, Created = 1, Updated = 2 }
    @LoginFailureCode int output, -- Optional. Set by CK.sAuthUserOnLogin if login is rejected.
    @LoginFailureReason nvarchar(255) output -- Optional.
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

    if @ActorId is null or @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId is null or @UserId < 0 throw 50000, 'Argument.InvalidUserId', 1;
	if @SimpleCode is null throw 50000, 'Argument.NullSimpleCode', 1;
	if @UserId = 0 and (@Mode <> 2 or @CheckLogin = 0) throw 50000, 'Argument.ForUserIdZeroModeMustBeUpdateOnlyWithLogin', 1;
	--[beginsp]

	declare @ActualUserId int;
	declare @LastLoginTime datetime2(2);
	declare @Now datetime2(2) = sysutcdatetime();

	select	@ActualUserId = UserId,
            @LastLoginTime = LastLoginTime
		from CK.tUserSimpleCode 
		where SimpleCode = @SimpleCode;

	--<PreCreateOrUpdate revert /> 

	if @ActualUserId is null
	begin
		if (@Mode&1) <> 0 -- CreateOnly or CreateOrUpdate
		begin
            set @LastLoginTime = '00010101';
			--<PreCreate revert /> 

			-- Unique constraint on SimpleCode will detect any existing UserId/SimpleCode clashes.
			insert into CK.tUserSimpleCode( UserId, SimpleCode, LastLoginTime ) 
				select	@UserId, 
						@SimpleCode, 
						@LastLoginTime;

			set @UCResult = 1; -- Created

			--<PostCreate />
		end
		else set @UCResult = 0; -- None
	end
	else
	begin
		-- Updating an existing registration.
		if (@Mode&2) <> 0 -- UpdateOnly or CreateOrUpdate
		begin
			-- When updating, we may be in "login mode" if @UserId is 0.
			-- But if we are not, the provided @UserId must match the actual one.
			if @UserId = 0 set @UserId = @ActualUserId;
			else if @UserId <> @ActualUserId throw 50000, 'Argument.UserIdAndSimpleCodeMismatch', 1;

            -- We have nothing to update since in case of login, LastLoginTime must be set
            -- after having called CK.sAuthUserOnLogin.
            -- This fake update is used as a placeholder for any actual updates that may be
            -- injected by other packages.
			update CK.tUserSimpleCode set 
					LastLoginTime = LastLoginTime 
				where UserId = @ActualUserId and SimpleCode = @SimpleCode;
			set @UCResult = 2; -- Updated
		end
		else set @UCResult = 0; -- None 
	end

    --<PostCreateOrUpdate />

	if @CheckLogin = 1
	begin
        -- If the user is not registered and we did not create it @LastLoginTime is null.
        if @LastLoginTime is null set @LoginFailureCode = 2; -- UnregisteredUser
        else
        begin
		    exec CK.sAuthUserOnLogin 'SimpleCode', @LastLoginTime, @UserId, @ActualLogin, @Now, @LoginFailureCode output, @LoginFailureReason output;  
            if @ActualLogin = 1 and @LoginFailureCode is null
            begin
			    update CK.tUserSimpleCode set LastLoginTime = @Now
                    where UserId = @UserId and SimpleCode = @SimpleCode;
            end
        end
	end
    else set @LoginFailureCode = 0; -- None

	--[endsp]
end
