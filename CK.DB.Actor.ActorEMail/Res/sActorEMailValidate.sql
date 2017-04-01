-- SetupConfig: { "Requires": "CK.sActorEMailAdd" }
--
-- Validates an email by uptating its ValTime to the current sysutdatetime().
-- Validating a non existing email is silently ignored.
-- If the current primary mail is not validated, this newly validated email becomes
-- the primary one.
-- 
create procedure CK.sActorEMailValidate
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

	update CK.tActorEMail set ValTime = sysutcdatetime() where ActorId = @UserOrGroupId and EMail = @EMail;
	if @@RowCount = 1
	begin
		declare @ExistingPrimaryEMail nvarchar(255);
		select @ExistingPrimaryEMail = EMail 
			from CK.tActorEMail
			where ActorId = @UserOrGroupId and IsPrimary = 1 and ValTime = '0001-01-01';
		if @ExistingPrimaryEMail is not null
		begin
			exec CK.sActorEMailAdd @ActorId, @UserOrGroupId, @EMail, @IsPrimary = 1; 
		end
	end

	--<PostEMailValidate />
	
	--[endsp]
end

