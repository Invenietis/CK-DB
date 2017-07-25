-- SetupConfig: { } 
-- 
create procedure CK.sAuthProviderRegister
( 
	@ActorId int, 
	@ProviderName varchar(64),
	@UserProviderSchemaTableName nvarchar(128),
	@IsMultiScheme bit,
	@AuthProviderIdResult int output
)
as 
begin
	--[beginsp]

	--<PreCreate revert />

	insert CK.tAuthProvider( ProviderName, UserProviderSchemaTableName, IsMultiScheme ) 
		values( @ProviderName, @UserProviderSchemaTableName, @IsMultiScheme );
	set @AuthProviderIdResult = scope_identity();

	--<PostCreate />

	--[endsp]
end
