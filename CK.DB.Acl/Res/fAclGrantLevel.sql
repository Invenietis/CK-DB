-- Version = 2.0.0
create function CK.fAclGrantLevel
(
    @ActorId Int,
    @AclId Int
)
returns TinyInt
as begin
	return IsNull( -- Is the actor the system or a member of the system group?
			(select 127 from CK.tActorProfile with(nolock) 
					where ActorId = @ActorId and GroupId = 1), 
			IsNull( -- Is there any GrantLevel associated to the groups it belongs to (plus the anonymous)?
				(select case when max(t.GrantLevel) > 127 then 255-max(t.GrantLevel) else max(t.GrantLevel) end
						from 
						(	select c.GrantLevel
								from CK.tAclConfig c with(nolock)
								inner join CK.tActorProfile groups with(nolock) on groups.GroupId = c.ActorId
								where groups.ActorId = @ActorId and c.AclId = @AclId
							union all
							select c.GrantLevel 
								from CK.tAclConfig c with(nolock) where c.ActorId = 0 and c.AclId = @AclId
						) as t
				),
				-- Else, GrantLevel is zero: denied
				0 ));
end