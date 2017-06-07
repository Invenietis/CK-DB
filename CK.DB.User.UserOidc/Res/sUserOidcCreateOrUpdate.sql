-- SetupConfig: {}
--
-- @SchemeSuffix can not be null. This is the key that identifies a Oidc user.
-- @Mode (flags): CreateOnly = 1, UpdateOnly = 2, CreateOrUpdate = 3, WithLogin = 4  
-- @Result: None = 0, Created = 1, Updated = 2
--
-- When @UserId = 0 we are in "login mode": 
--  - @Mode must be 2 or 6 (UpdateOnly or UpdateOnly+WithLogin).
--  - If the @SchemeSuffix/@Sub is found, we update the infos and output the found @UserId.
-- When @UserId is not 0, it must match with the one of the @SchemeSuffix/@Sub otherwise an exception is thrown
--  - When updating it means that there is a mismatch in the calling code.
--  - When creating it means that another user with the same @SchemeSuffix/@Sub account is already registered and
--    this should never happen.
--
-- When extending this procedure, on update null parameters must be left unchanged.
--
create procedure CK.sUserOidcCreateOrUpdate
(
	@ActorId int,
	@UserId int /*input*/output,
	@SchemeSuffix varchar(64),
    @Sub nvarchar(64),
	@Mode int, -- not null enum { "CreateOnly" = 1, "UpdateOnly" = 2, "CreateOrUpdate" = 3, "WithLogin" = 4, "IgnoreOptimisticKey" = 8 }
	@Result int output -- not null enum { None = 0, Created = 1, Updated = 2 }
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

    if @ActorId is null or @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId is null or @UserId < 0 throw 50000, 'Argument.InvalidUserId', 1;
	if @SchemeSuffix is null throw 50000, 'Argument.NullSchemeSuffix', 1;
    set @SchemeSuffix = LTrim(RTrim(@SchemeSuffix));
	if @Sub is null throw 50000, 'Argument.NullSub', 1;
    if @Mode is null or @Mode < 1 or @Mode > 3 throw 50000, 'Argument.InvalidMode', 1;
	if @UserId = 0 and @Mode <> 2 throw 50000, 'Argument.ModeMustBeUpdateOnlyForLogin', 1;
	--[beginsp]

	declare @ActualUserId int;
	declare @Now datetime2(2) = sysutcdatetime();

	select	@ActualUserId = UserId
		from CK.tUserOidc 
		where SchemeSuffix = @SchemeSuffix and Sub = @Sub;

	--<PreCreateOrUpdate revert /> 

	if @ActualUserId is null
	begin
		if (@Mode&1) <> 0 -- CreateOnly or CreateOrUpdate
		begin
			--<PreCreate revert /> 

			-- Unique constraint on SchemeSuffix/Sub will detect any existing UserId/ SchemeSuffix/Sub clashes.
			insert into CK.tUserOidc( UserId, SchemeSuffix, Sub, LastLoginTime ) 
				select	@UserId, 
						@SchemeSuffix, 
						@Sub, 
						case when  @ActualLogin = 1 then @Now else '0001-01-01' end;

			set @Result = 1; -- Created

			--<PostCreate />
		end
		else set @Result = 0; -- None
	end
	else
	begin
		-- Updating an existing registration.
		if (@Mode&2) <> 0 -- UpdateOnly or CreateOrUpdate
		begin
			-- When updating, we may be in "login mode" if @UserId is 0.
			-- But if we are not, the provided @UserId must match the actual one.
			if @UserId = 0 set @UserId = @ActualUserId;
			else if @UserId <> @ActualUserId throw 50000, 'Argument.UserIdAndSchemeSuffixSubMismatch', 1;

			update CK.tUserOidc set 
					LastLoginTime = case when  @ActualLogin = 1 then @Now else LastLoginTime end
				where UserId = @ActualUserId and SchemeSuffix = @SchemeSuffix and Sub = @Sub;
			set @Result = 2; -- Updated
		end
		else set @Result = 0; -- None 
	end
	if @Result <> 0 and @ActualLogin = 1
	begin
        if len(@SchemeSuffix) = 0 
        begin
            set @SchemeSuffix = 'Oidc';
        end
        else
        begin
            set @SchemeSuffix = 'Oidc.'+@SchemeSuffix;
        end
		exec CK.sAuthUserOnLogin @SchemeSuffix, @Now, @UserId;  
	end
	--<PostCreateOrUpdate /> 

	--[endsp]
end