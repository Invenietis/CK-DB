-- SetupConfig: { }
create procedure CK.sAclTypeCreate 
(
	@ActorId int,
	@AclTypeIdResult int output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	--<PreAclTypeCreate revert />

	insert into CK.tAclType( ConstrainedGrantLevel ) values ( 0 );
	set @AclTypeIdResult = SCOPE_IDENTITY();
	insert into CK.tAclTypeGrantLevel( AclTypeId, GrantLevel ) 
		 values ( @AclTypeIdResult, 0 ),	-- Blind
				( @AclTypeIdResult, 127 );	-- Administrator
	--<PostAclTypeCreate />

	--[endsp]
end

