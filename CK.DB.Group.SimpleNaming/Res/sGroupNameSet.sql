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

	--[beginsp]
	
	declare @GroupNameCorrected nvarchar(128);
	select @GroupNameCorrected = CK.fGroupNameComputeUnique( @GroupId, @GroupName );

	--<PreNameSet revert />

	update CK.tGroup set GroupName = @GroupNameCorrected where GroupId = @GroupId;

	--<PostNameSet />

	--[endsp]
end