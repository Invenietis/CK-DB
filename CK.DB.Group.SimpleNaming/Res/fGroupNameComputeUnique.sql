-- SetupConfig: {}
-- The @GroupId parameter must be set to the user identifier in case of a
-- rename (to handle the case of a rename with the same name). 
-- Set it to -1 to compute a user name for a new user.
create Function CK.fGroupNameComputeUnique
	(
		@GroupId	int,
		@GroupName	nvarchar(128)
	)
returns nvarchar(128) with SCHEMABINDING
as 
begin
	if not exists( select GroupId 
						from CK.tGroup 
						where GroupId <> @GroupId and GroupName = @GroupName ) 
	begin
		return @GroupName;
	end
	if len(@GroupName) > 123 set @GroupName = left( @GroupName, 123 );
	set @GroupName = @GroupName + ' (';
	declare @proposed nvarchar(128);
	declare @num int = 1;
	while @num <= 99 
	begin
		set @proposed = @GroupName + cast(@num as nvarchar(4)) + ')';
		if not exists( select GroupId 
							from CK.tGroup 
							where GroupId <> @GroupId and GroupName = @proposed ) 
		begin
			return @proposed;
		end
		set @num = @num+1;
	end
	return null;
end
