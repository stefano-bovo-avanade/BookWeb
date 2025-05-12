var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    var parameter = url.substring(url.indexOf("=") + 1, url.length);

    switch (parameter) {
        case "inprocess":
            loadDataTable("inprocess");
            break;
        case "pending":
            loadDataTable("pending");
            break;
        case "completed":
            loadDataTable("completed");
            break;
        case "approved":
            loadDataTable("approved");
            break;
        default:
            loadDataTable("all");
            break;
    }
});
function loadDataTable(status)
{
    dataTable = $('#tblData').DataTable({
        ajax:
        {
            url:  '/admin/Order/GetAll?status=' + status,
            dataSrc: 'data'
        },
        columns : [
            { data: 'id', "width":"10%" , title: "Id" },
            { data: 'applicationUser.name', "width": "15%", title: "Nome" },
            { data: 'applicationUser.phoneNumber', "width": "20%", title: "Numero di telefono" },
            { data: 'applicationUser.email', "width": "20%", title: "Email" },
            { data: 'orderStatus', "width": "10%", title: "Stato" },
            { data: 'orderTotal', "width": "10%", title: "Totale" },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                            </div>`
                },
                "width": "25%"
            }
            
        ]
    });
}



