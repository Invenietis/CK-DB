-- SetupConfig: { } 
-- 
create procedure CK.sAuthProviderRegister
( 
	@ActorId int, 
	@ProviderName varchar(64),
	@UserProviderSchemaTableName nvarchar(128),
	@IsMultiScheme bit,
	@AuthProviderResult int output
)
as 
begin
	--[beginsp]

	--<PreCreate revert />

	insert CK.tAuthProvider( ProviderName, UserProviderSchemaTableName, IsEnabled, IsMultiScheme ) 
		values( @ProviderName, @UserProviderSchemaTableName, 1, @IsMultiScheme );
	set @AuthProviderResult = scope_identity();

	--<PostCreate />

	--[endsp]
end
