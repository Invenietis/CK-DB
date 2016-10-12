-- Version = 1.2.0, Requires = { CK.sGroupUserAdd, CK.sZoneUserAdd, CK.sAclCreate }
create procedure CK.sZoneCreate 
(
	@ActorId int,
	@ParentZoneId int = 0,
	@ZoneIdResult int output
)
as
begin
    if @ActorId <= 0 raiserror( 'Security.AnonymousNotAllowed', 16, 1 );

	--[beginsp]

	--<Extension Name="Zone.PreZoneCreate">

	declare @LeftNumber int;
	declare @RightNumber int;
	declare @Depth int;

	 select @LeftNumber = LeftNumber, @RightNumber = RightNumber, @Depth = Depth
        from CK.tZone
        where ZoneId = @ParentZoneId;
	if @LeftNumber is null raiserror( 'Zone.InvalidParentId', 16, 1 );

    update CK.tZone
           set RightNumber = RightNumber + 2
           where RightNumber >= @RightNumber;
    update CK.tZone
           set LeftNumber = LeftNumber + 2
           where LeftNumber > @RightNumber;
    set @LeftNumber = @RightNumber;
    set @RightNumber = @RightNumber + 1;
    set @Depth = @Depth + 1;

	--</Extension>

	-- A Zone is Group.  
	exec CK.sGroupCreate @ActorId, 0, @ZoneIdResult output;
		
	-- Do create the Zone
	insert into CK.tZone( 
			ZoneId, AdministratorsGroupId, LeftNumber, RightNumber, Depth
		) values ( 
			@ZoneIdResult, 1, @LeftNumber, @RightNumber, @Depth
		);

	-- The Zone of this group is the Parent Zone itself...
	-- ...so that this Group (the child group) appears to be a group like any other.
	update CK.tGroup set ZoneId = @ParentZoneId where GroupId = @ZoneIdResult;

	-- Creates the first group of the Zone. This group is the Administrators Group of the Zone.
	declare @AdministratorsGroupId int;
	exec CK.sGroupCreate @ActorId, @ZoneIdResult, @AdministratorsGroupId output;
		
	update CK.tZone set AdministratorsGroupId = @AdministratorsGroupId where ZoneId = @ZoneIdResult;

	if @ActorId > 1
	begin
		declare @Done bit;
		-- The current actor becomes a member of the newly created Zone.
		exec CK.sZoneUserAdd @ActorId, @ZoneIdResult, @ActorId;
		
		-- The current actor becomes an administrator of the newly created Zone.
		exec CK.sGroupUserAdd @ActorId, @AdministratorsGroupId, @ActorId;
	end

	-- We create the BackOffice group
	declare @backOfficeGroupId int;
	declare @prospectionGroupId int;

	exec CK.sGroupCreate @ActorId, 0, @backOfficeGroupId output;
	update CK.tGroup set ZoneId = @ZoneIdResult, GroupName = 'BackOffice' where GroupId = @backOfficeGroupId;
	update CK.tZone set BackOfficeGroupId = @backOfficeGroupId where ZoneId = @ZoneIdResult;

	-- We create the Prospection group
	exec CK.sGroupCreate @ActorId, 0, @prospectionGroupId output;
	update CK.tGroup set ZoneId = @ZoneIdResult, GroupName = 'Prospection' where GroupId = @prospectionGroupId;
	update CK.tZone set ProspectionGroupId = @prospectionGroupId where ZoneId = @ZoneIdResult;

	--<Extension Name="Zone.PostZoneCreate">

	--From: ACL
		
	-- We declare the BackOfficeAcl
	declare @AclId int;
	exec CK.sAclCreate @ActorId, @AclId output;
	update CK.tZone set BackOfficeAclId = @AclId where ZoneId = @ZoneIdResult;
	-- The administrator group has full control on the security Zone ACL.
	exec CK.sAclGrantSet @ActorId, @AclId, @AdministratorsGroupId, 'Acl.AdministratorGroup', 127;
	-- New user added to group will be editor
	exec CK.sAclGrantSet @ActorId, @AclId, @backOfficeGroupId, 'Acl.BackOfficeGroup', 64;


	-- We declare the ProspectionAcl
	exec CK.sAclCreate @ActorId, @AclId output;
	update CK.tZone set ProspectionAclId = @AclId where ZoneId = @ZoneIdResult;
	-- The administrator group has full control on the security Zone ACL.
	exec CK.sAclGrantSet @ActorId, @AclId, @AdministratorsGroupId, 'Acl.ZoneAdministratorGroup', 127;
	-- New user added to group will be editor
	exec CK.sAclGrantSet @ActorId, @AclId, @prospectionGroupId, 'Acl.ProspectionGroup', 64;

	--/From

	--</Extension>

	--[endsp]
end
 
