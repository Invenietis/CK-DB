-- SetupConfig: { "Requires": [ "CK.fGroupNameComputeUnique" ] }
--
-- Sets a Group's name.
--
create procedure CK.sGroupNameSet
(
	@ActorId int,
	@GroupId int,
	@GroupName nvarchar(128) /*input*/output
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupName is null throw 50000, 'GroupName.CanNotBeNull', 1;

	--[beginsp]
	
	declare @GroupNameCorrected nvarchar(128);
	select @GroupNameCorrected = CK.fGroupNameComputeUnique( @GroupId, @GroupName );

	--<PreNameSet revert />

	if @GroupNameCorrected is null throw 50000, 'GroupName.NameClash', 1;
	update CK.tGroup set GroupName = @GroupNameCorrected where GroupId = @GroupId;

	--<PostNameSet />

	set @GroupName = @GroupNameCorrected;

	--[endsp]
end