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
	-- Use exec to call a function: this enables default parameters 
	-- to be applied without writing DEFAULT for each of them...
	-- Default parameters at the function call site should be handled by the 
	-- global resolution. Once available, this should be rewritten with the  
	-- more usual and standard syntax:
	--		select @GroupNameCorrected = CK.fGroupNameComputeUnique( @GroupId, @GroupName );
	-- That should be changed by CK-Database into:
	--		select @GroupNameCorrected = CK.fGroupNameComputeUnique( @GroupId, @GroupName, DEFAULT );
	-- (With as much DEFAULT as needed.)
	exec @GroupNameCorrected = CK.fGroupNameComputeUnique @GroupId, @GroupName;

	--<PreNameSet revert />

	if @GroupNameCorrected is null throw 50000, 'GroupName.NameClash', 1;
	update CK.tGroup set GroupName = @GroupNameCorrected where GroupId = @GroupId;

	--<PostNameSet />

	set @GroupName = @GroupNameCorrected;

	--[endsp]
end