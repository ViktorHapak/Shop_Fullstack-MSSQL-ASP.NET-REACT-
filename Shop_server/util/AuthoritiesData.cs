namespace Shop_server.util
{
    public class AuthoritiesData
    {
        public Boolean AddProduct { get; init; } = false;
        public Boolean UpdateProduct { get; init; } = false;
        public Boolean DeleteProduct { get; init; } = false;

        public Boolean AddDepartment { get; init; } = false;
        public Boolean DeleteDepartment { get; init; } = false;

        public Boolean ReplenishStack { get; init; } = true;
        public Boolean ReduceStack { get; init; } = false;

        public Boolean ConfirmOrder { get; init; } = true;
        public Boolean RejectOrder { get; init; } = true;
        public Boolean CleanExpired { get; init; } = true;

        public Boolean SetNewDay { get; init; } = true;
        public Boolean ExportDatas { get; init; } = true;

        public Boolean DeleteUser { get; init; } = false;
    }
}
