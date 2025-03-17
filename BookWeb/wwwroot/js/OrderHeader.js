var dataTable;
$(document).ready(function () {
    var dataTable = $('#tblData');
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax:
        {
            url: '/admin/OrderHeader/GetAll',
            dataSrc: 'data'
        },
        columns: [
            { data: 'ApplicationUserId', title: "Id Application User" },
            { data: 'OrderDate', title: "Data ordine" },
            { data: 'ShippingDate', title: "Data spedizione" },
            { data: 'OrderTotal', title: "Totale ordine" },
            { data: 'OrderStatus', title: "Stato ordine" },
            { data: 'PaymentStatus', title: "Stato pagamento" },
            { data: 'TrackingNumber', title: "Numero di tracking" },
            { data: 'Carrier', title: "Spedizioniere" },
            { data: 'PaymentDate', title: "Data pagamento" },
            { data: 'PaymentDueDate', title: "Data scadenza pagamento" },
            { data: 'PaymentIntentId', title: "Id pagamento" },
            { data: 'PhoneNumber', title: "Numero di telefono" },
            { data: 'Name', title: "Nome" },
            { data: 'StreetAddress', title: "Indirizzo" },
            { data: 'City', title: "Città" },
            { data: 'State', title: "Stato" },
            { data: 'PostalCode', title: "Codice postale" }
            //{
            //    data: 'id',
            //    title: "",
            //    orderable: false,
            //    render: function (data) {
            //        return `<div class="w-75 btn-group" role="group">
            //                    <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
            //                        <i class="bi bi-pencil-square"></i>
            //                        Edit
            //                    </a>
            //                    <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2">
            //                        <i class="bi bi-trash-fill"></i>
            //                        Delete
            //                    </a>
            //                </div>`
            //    },
            //    "width": "25%"
            //}
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax(
                {
                    url: url,
                    type: 'Delete',
                    success: function (data) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                });
        }
    });
}


