$(document).ready(function () {
    var dataTable = $('#tblData');
    loadDataTable();
});
function loadDataTable()
{
    dataTable = $('#tblData').DataTable({
        ajax:
        {
            url: '/admin/Product/GetAll',
            dataSrc: 'data'
        },
        columns : [
            { data: 'title', "width":"15%" },
            { data: 'isbn', "width": "15%" },
            { data: 'price', "width": "15%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "15%" }]
    });
}


