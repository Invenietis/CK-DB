-- Version = *
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
	if not exists( select GroupId from CK.tGroup where GroupId <> @GroupId and GroupName = @GroupName ) 
		return @GroupName;
	if len(@GroupName) > 122 set @GroupName = left( @GroupName, 122 );
	set @GroupName = @GroupName + ' (';
	declare @proposed nvarchar(128);
	declare @num int = 1;
	while @num < 1000 
	begin
		set @proposed = @GroupName + cast(@num as nvarchar(4)) + ')';
		if not exists( select GroupId from CK.tGroup where GroupName = @proposed ) 
			return @proposed;
		set @num = @num+1;
	end
	return @proposed
end
