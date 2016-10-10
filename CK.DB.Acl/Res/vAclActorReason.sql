-- SetupConfig: {}
-- Kernel Acl view that gives for each Actor and each Acl the ActorId (GroupId or UserId) that
-- is responsible for the actual GrantLevel.
-- The ReasonId (that is the ActorId) is positive for a Grant and negative for a Deny.
alter view CK.vAclActorReason with SCHEMABINDING
as
	select 	acl.AclId,
		ActorId = a.ActorId, 
		ReasonId = IsNull( 
						-- Is the actor the system or a member of the system group? 
						-- If yes, ReasonId is group 1 (system).
						(select 1 from CK.tActorProfile with(nolock) 
								where ActorId = a.ActorId and GroupId = 1),
						IsNull( -- Is there any GrantLevel associated to the groups it belongs to (plus anonymous)? 
								-- If yes, Reason is the best group identifier (one of those that decide):
								--  positive if the GrantLevel is the result of a normal grant level.
								--  negative (i.e. -GroupId) if the group is restricting the level (deny, ie > 127).
								-- Note: it works because Anonymous can not deny: when ActorId = 0 it is a necessarily a grant. 
							(select top 1 case when t.GrantLevel <= 127 then abs(t.ActorId) else -abs(t.ActorId) end
									from 
									(	select c.GrantLevel, c.ActorId
											from CK.tAclConfig c with(nolock)
											inner join CK.tActorProfile groups with(nolock) on groups.GroupId = c.ActorId
											where groups.ActorId = a.ActorId and c.AclId = acl.AclId
										union all
										select c.GrantLevel, 0
											from CK.tAclConfig c with(nolock) where c.ActorId = 0 and c.AclId = acl.AclId
									) as t
 								order by t.GrantLevel desc, t.ActorId desc ),
							-- Else, GrantLevel is zero: denied. Reason is -1: no rights.
							-1 )
				)
		from CK.tAcl acl with(nolock)
		cross join CK.tActor a with(nolock);



