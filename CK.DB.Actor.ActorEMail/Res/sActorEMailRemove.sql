-- SetupConfig: { "Requires": "CK.sActorEMailAdd" }
--
-- Removes an email.
-- Removing an unexisting email is silently ignored.
-- When the removed email is the primary one, the most recently validated email becomes
-- the new primary one.
-- By default, this procedure allows the removal of the only actor's email.
--
create procedure CK.sActorEMailRemove
(
	@ActorId int,
	@UserOrGroupId int,
	@EMail nvarchar(255)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @EMail is null throw 50000, 'Argument.NullEMail', 1;
	set @EMail = rtrim(ltrim(@EMail));
	if len(@EMail) = 0 throw 50000, 'Argument.EmptyEMail', 1;

	--[beginsp]

	declare @IsPrimary bit;
	select @IsPrimary = IsPrimary from CK.tActorEMail where ActorId = @UserOrGroupId and EMail = @EMail;
	if @IsPrimary is not null
	begin
		--<PreDelete reverse />
		delete CK.tActorEMail where ActorId = @UserOrGroupId and EMail = @EMail;
		if @IsPrimary = 1
		begin
			declare @SetNewPrimary bit;
			select top 1 @EMail = EMail from CK.tActorEMail where ActorId = @UserOrGroupId order by ValTime desc;
			if @@RowCount = 1 set @SetNewPrimary = 1 else set @SetNewPrimary = 0;
			-- Injected code here may decide to throw if @SetNewPrimary is 0
			-- if a primary email must always exist (this check may also be done in PreDelete above).		
			--<PreSetNewPrimary reverse />
			if @SetNewPrimary = 1
			begin
				exec CK.sActorEMailAdd @ActorId, @UserOrGroupId, @EMail, @IsPrimary = 1; 
			end
			--<PostSetNewPrimary />
		end
		--<PostDelete />
	end

	
	--[endsp]
end

