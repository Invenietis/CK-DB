-- SetupConfig: { }
--
-- Both @FailureCode and @FailureReason can be set.
-- 
-- @LastLoginTime:  Last login time ('0001-01-01' for first login).</param>
-- @LoginTimeNow:   Current login time. This is the exact time that will become, on success, the LastLoginTime
--                  in the provider table.
-- @ActualLogin:    True for an actual login. False otherwise: only checks must be done.
-- @FailureCode:    To reject login, set this to a non null value (should be greater to 0).
-- @FailureReason:  Optional (may be deduced from @FailureCode). If set to a non null string and @FailureCode
--                  is not set, @FailureCode is set to 1 (Unspecified).
--
--  On output these 2 variables are normalized to be both null if login succeeds or non null on failure.
--
create procedure CK.sAuthUserOnLogin
( 
	@Scheme varchar(64),
	@LastLoginTime datetime2(2),
	@UserId int,
    @ActualLogin bit,
	@LoginTimeNow datetime2(2),
    @FailureCode int output,
    @FailureReason nvarchar(255) output
)
as 
begin
	--[beginsp]

    --<CheckLoginFailure />

    if @FailureReason is not null or @FailureCode is not null
    begin
        -- Normalize @FailureCode: always positive.
        if @FailureCode is null or @FailureCode = 0 set @FailureCode = 1; -- Unspecified
        else if @FailureCode < 0 set @FailureCode = -@FailureCode;
        -- Normalize @FailureReason (trim) and back to null if empty.
        set @FailureReason = rtrim(ltrim(@FailureReason));
        if len(@FailureReason) = 0 set @FailureReason = null;
    end

    --<PostCheck />

    if @ActualLogin = 1
    begin
        if @FailureCode is null
        begin
            declare @unused1 int;
            --<LoginSucceed />
        end
        else
        begin
            declare @unused2 int;
            --<LoginFailed />
        end
    end

	--[endsp]
end
