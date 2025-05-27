var dataTable;
$(document).ready(function () {

    var currentStatus = 'all';
    $('.filter-btn').on('click', function () {

        $('.filter-btn')
            .removeClass('active text-white bg-primary shadow-none')
            .addClass('text-primary');
        $(this)
            .removeClass('text-primary')
            .addClass('active text-white bg-primary shadow-none');

        currentStatus = $(this).data('status');
        dataTable.ajax.reload();
    });

    dataTable = $("#tblData").DataTable({
        ajax:
        {
            url: '/admin/Order/GetAll',
            dataSrc: 'data',
            data: function (d) {
                d.status = currentStatus;
            },
        },
        columns: [
            { data: 'id', "width": "10%", title: "Id" },
            { data: 'applicationUser.name', "width": "15%", title: "Nome" },
            { data: 'applicationUser.phoneNumber', "width": "20%", title: "Numero di telefono" },
            { data: 'applicationUser.email', "width": "20%", title: "Email" },
            { data: 'orderStatus', "width": "10%", title: "Stato" },
            { data: 'orderTotal', "width": "10%", title: "Totale" },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/order/Details?orderId=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                        </div>`
                },
                "width": "25%"
            }

        ]
    });
});




