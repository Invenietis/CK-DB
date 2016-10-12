-- Version = *, Requires= { CK.vGroup }
create view CK.vZone
as
	select	z.ZoneId, 
			z.AdministratorsGroupId,
			z.BackOfficeAclId,
			z.BackOfficeGroupId,
			z.ProspectionAclId,
			z.ProspectionGroupId,
			g.UserCount,
			GroupCount = (select count(*)-1 from CK.tGroup g where g.ZoneId = z.ZoneId),
			--z.AclId,
			--<Hierarchy name="">
			ChildCount = (z.RightNumber - z.LeftNumber) / 2,
			ParentId = g.ZoneId,
			Depth = z.Depth,
			OrderByKey = z.LeftNumber
			--</Hierarchy>
	from CK.tZone z
	inner join CK.vGroup g on g.GroupId = z.ZoneId
