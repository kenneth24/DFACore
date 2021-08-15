
function ResendApplication(applicationCode) {
    alert(applicationCode);
    $('#loading').show();
    $.ajax({
        url: urlResend,
        method: 'GET',
        cache: false,
        contentType: 'application/json; charset=utf-8',
        data: { applicationCode: applicationCode },
        success: function (result) {

            $('#loading').hide();
            toastr.success('Apostille Application has been successfully resend.', 'Success', { timeOut: 5000 })
            //toastr.success('Success');
        },
        error: function () {
            $('#loading').hide();
            toastr.error('Something went wrong!', 'Error', { timeOut: 5000 });

        }
    });
};

function ViewPDFApplication(applicationCode) {
    alert(applicationCode);
    
    window.open(urlView + "?applicationCode=" + applicationCode, '_blank');
};

$(document).ready(function () {

    

    var oTable = $("#customerDatatable").dataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        "ajax": {
            /*"url": "/Administration/GetAllApplicantRecordForDT",*/
            url: urlHome,
            "type": "POST",
            "datatype": "json"
        },
        'columnDefs': [
            {
                'targets': 0,
                'checkboxes': {
                    'selectRow': true
                }
            }
        ],
        //"columnDefs": [{
        //    //"targets": [0],
        //    //"visible": false,
        //    //"searchable": false
        //    "targets": 2,
        //    "render": $.fn.dataTable.render.moment('YYYY/MM/DD')
        //}],
        "ordering": false,
        "searchDelay": 350,
        "columns": [
            { "data": "applicationCode", "name": "chk", "autoWidth": true },
            { "data": "applicationCode", "name": "ApplicationCode", "autoWidth": true },
            { "data": "scheduleDate", "name": "Schedule Date", "autoWidth": true, },
            { "data": "dateCreated", "name": "Date Created", "autoWidth": true },
            { "data": "email", "name": "Email", "autoWidth": true },
            { "data": "documentOwner", "name": "Document Owner", "autoWidth": true },
            { "data": "contactNumber", "name": "Contact No.", "autoWidth": true },
            { "data": "nameOfRepresentative", "name": "Authorized Rep.", "autoWidth": true },
            { "data": "representativeContactNumber", "name": "Contact No.", "autoWidth": true },
            { "data": "consularOffice", "name": "Consular Office", "autoWidth": true },
            { "data": "countryDestination", "name": "Destination", "autoWidth": true },
            { "data": "documents", "name": "Documents", "autoWidth": true },
            { "data": "quantity", "name": "Qty", "autoWidth": true },
            { "data": "transaction", "name": "Transaction", "autoWidth": true },
            {
                "data": "applicationCode", "width": "50px", "render": function (data)
                { return "<a href='#' class='btn btn-info' onclick=ResendApplication('" + data + "'); >Resend</a> <a href='#' class='btn btn-danger' onclick=ViewPDFApplication('" + data + "'); >View</a>"; }
                /*"render": function (data) { return "<a href='#' class='btn btn-info' onclick=ResendApplication('" + data + "'); >Resend</a> <a href='#' class='btn btn-danger' onclick=DeleteCustomer('" + row.applicationCode + "'); >View</a>"; }*/
            },
           
        ]
    });

    $('#customerDatatable_filter input').unbind();
    $('#customerDatatable_filter input').bind('keyup', function (e) {
        if (e.keyCode == 13) {
            oTable.fnFilter(this.value);
        }
    });

    //$('#frm-example').on('submit', function (e) {
    //    var form = this;

    //    var rows_selected = table.column(0).checkboxes.selected();

    //    // Iterate over all selected checkboxes
    //    $.each(rows_selected, function (index, rowId) {
    //        // Create a hidden element
    //        $(form).append(
    //            $('<input>')
    //                .attr('type', 'hidden')
    //                .attr('name', 'id[]')
    //                .val(rowId)
    //        );
    //    });
    //});

    



    $("#CancelAppointment").click(function () {
        var table = $('#customerDatatable').DataTable();
        var rows_selected = table.column(0).checkboxes.selected();
        if (rows_selected === "") {
            toastr.error('Please select atleast 1 record!', 'Warning', { timeOut: 5000 });
            return;
        }
        else {
            //alert("'" + rows_selected.join("','") + "'");

            $('#loading').show();
            $.ajax({
                url: urlHomeCancel,
                method: 'GET',
                cache: false,
                contentType: 'application/json; charset=utf-8',
                data: { applicationCode: "'" + rows_selected.join("','") + "'" },
                success: function (result) {

                    $('#loading').hide();
                    toastr.success('Apostille application has been successfully cancelled.', 'Success', { timeOut: 5000 })
                    //toastr.success('Success');
                    table.columns().checkboxes.deselect(true);
                    table.ajax.reload();
                    //location.reload()
                },
                error: function () {
                    $('#loading').hide();
                    toastr.error('Something went wrong!', 'Error', { timeOut: 5000 });

                }

            });

        }
        
        
    });

    $('#frm-example').on('submit', function (e) {
        var form = this;

        var rows_selected = table.column(0).checkboxes.selected();

        // Iterate over all selected checkboxes
        $.each(rows_selected, function (index, rowId) {
            // Create a hidden element 
            $(form).append(
                $('<input>')
                    .attr('type', 'hidden')
                    .attr('name', 'id[]')
                    .val(rowId)
            );
        });

        // FOR DEMONSTRATION ONLY
        // The code below is not needed in production

        // Output form data to a console     
        $('#example-console-rows').text(rows_selected.join(","));
        alert(rows_selected.join(","));
        // Output form data to a console     
        $('#example-console-form').text($(form).serialize());

        // Remove added elements
        $('input[name="id\[\]"]', form).remove();

        // Prevent actual form submission
        e.preventDefault();
    });
});