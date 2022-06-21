-- SetupConfig: {}
--
-- Updates a LastName or FirstName of a User.

create procedure CK.sNamedUserSetNames
    @ActorId int, 
    @UserId int, 
    @FirstName nvarchar(255),
    @LastName nvarchar(255)
as 
begin 
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

    --[beginsp]
    update CK.tUser
        set FirstName = case when @FirstName is not null then @FirstName else FirstName end, 
            LastName = case when @LastName is not null then @LastName else LastName end
        where UserId = @UserId
    --[endsp]
end
