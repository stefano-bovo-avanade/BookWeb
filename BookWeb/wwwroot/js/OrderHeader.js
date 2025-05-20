$(document).ready(function () {
    if ($("#tblData").length) {
        loadDataTable();
    }
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        scrollY: '400px',
        scrollX: true,
        scrollCollapse: true,
        paging: true,
        ajax:
        {
            url: '/admin/OrderHeader/GetAll',
            dataSrc: 'data'
        },
        columns: [
            { data: 'id', visible: false },
            { data: 'applicationUserId', width: '300px', title: "Id Application User" },
            {
                data: 'orderDate',
                width: '200px',
                title: 'Data ordine',
                render: function (data) {
                    const date = new Date(data);
                    return date.toLocaleDateString('it-IT') + ' ' + date.toLocaleTimeString('it-IT');
                }
            },
            {
                data: 'shippingDate',
                width: '200px',
                title: 'Data spedizione',
                render: function (data) {
                    const date = new Date(data);
                    return date.toLocaleDateString('it-IT') + ' ' + date.toLocaleTimeString('it-IT');
                }
            },
            { data: 'orderTotal', width: '150px', title: "Totale ordine" },
            { data: 'orderStatus', width: '150px', title: "Stato ordine" },
            { data: 'paymentStatus', title: "Stato pagamento" },
            { data: 'trackingNumber', width: '150px', title: "Numero di tracking" },
            { data: 'carrier', title: "Spedizioniere" },
            {
                data: 'paymentDate',
                width: '200px',
                title: 'Data pagamento',
                render: function (data) {
                    const date = new Date(data);
                    return date.toLocaleDateString('it-IT') + ' ' + date.toLocaleTimeString('it-IT');
                }
            },
            {
                data: 'paymentDueDate',
                width: '200px',
                title: 'Data scadenza pagamento',
                render: function (data) {
                    const date = new Date(data);
                    return date.toLocaleDateString('it-IT') + ' ' + date.toLocaleTimeString('it-IT');
                }
            },
            { data: 'paymentIntentId', title: "Id pagamento" },
            { data: 'phoneNumber', width: '150px', title: "Numero di telefono" },
            { data: 'name', title: "Nome" },
            { data: 'streetAddress', title: "Indirizzo" },
            { data: 'city', title: "Città" },
            { data: 'state', title: "Stato" },
            { data: 'postalCode', width: '150px', title: "Codice postale" }
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


