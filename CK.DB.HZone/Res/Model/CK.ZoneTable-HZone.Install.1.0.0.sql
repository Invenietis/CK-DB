--[beginscript]

alter table CK.tZone add HierarchicalId hierarchyid null;

--[endscript]

--[beginscript]

update CK.tZone set HierarchicalId = hierarchyid::GetRoot() where ZoneId = 0;

declare @rootId hierarchyid, @nodeId hierarchyid;

select @rootId = HierarchicalId, @nodeId = null from CK.tZone where ZoneId = 0;
update CK.tZone set @nodeId = HierarchicalId = @rootId.GetDescendant( @nodeId, null ) where ZoneId > 0;

alter table CK.tZone alter column HierarchicalId hierarchyid not null;

create unique index IX_CK_tZone_HierarchicalId on CK.tZone(HierarchicalId);

--[endscript]
