-- SetupConfig: { } 
-- 
create procedure CK.sAuthProviderIsEnableSet
( 
	@ActorId int, 
	@ProviderName varchar(64),
	@IsEnabled bit = 1
)
as 
begin
	--[beginsp]

	--<PreSet revert />

	update CK.tAuthProvider set IsEnabled = @IsEnabled where ProviderName = @ProviderName;

	--<PostSet />

	--[endsp]
end
