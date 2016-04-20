-- Version = *
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
	
	--<Extension Name="Group.PreNameSet" />

	select @GroupName = CK.fGroupNameComputeUnique( @GroupId, @GroupName );
	update CK.tGroup set GroupName = @GroupName where GroupId = @GroupId;

	--<Extension Name="Group.PostNameSet" />

	--[endsp]
end