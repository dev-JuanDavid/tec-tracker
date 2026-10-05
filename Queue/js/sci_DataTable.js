let _dtTable = {};

_dtTable.GetData = (id, url) => {

    _dtTable.clear(id);

    $.ajax({
        url: url,
        dataType: 'JSON',
        type: 'GET',
        contentType: "application/json; charset=utf-8",
    }).done((data) => {
        console.log(data);
        let tr = [];
        data.forEach((e, idx) => {
            tr.push(`<tr><td>${e.Username}</td><td class="text-bold">${e.TypeOfEvent}</td><td class="text-success text-bold">${e.FileName}</td><td>${e.FechaInsercion}</td><td onclick="javascript:detail(${idx})"  data-toggle="tooltip" data-placement="top" title="Ver Detalle"><a class="btn btn-outline-secondary"><i class="glyphicon glyphicon-dashboard"></i></a></td></tr>`);
        });
        $("#thead").after(`<tbody class="text-center">${tr}</tbody>`);
        _dtTable.init(id);
        _dtTable.dataArray = data;
    }).fail((jqXHR) => {
        console.log(jqXHR);
    });
}

_dtTable.GetDataLocation = (id, url) => {

    _dtTable.clear(id);

    $.ajax({
        url: url,
        dataType: 'JSON',
        type: 'GET',
        contentType: "application/json; charset=utf-8",
    }).done((data) => {
        console.log(data);
        let tr = [];
        data.forEach((e, idx) => {
            tr.push(`<tr><td>${e.Name}</td><td class="text-bold">${e.User}</td><td class="text-success text-bold">${e.Date}</td><td>${e.Longitude}</td><td>${e.Latitude}</td></tr>`);
        });
        $("#thead").after(`<tbody class="text-center">${tr}</tbody>`);
        _dtTable.init(id);
        _dtTable.dataArray = data;
    }).fail((jqXHR) => {
        console.log(jqXHR);
    });
}

_dtTable.GetDataParamaterSystem = (id, url) => {

    _dtTable.clear(id);

    $.ajax({
        url: url,
        dataType: 'JSON',
        type: 'GET',
        contentType: "application/json; charset=utf-8",
    }).done((data) => {
        console.log(data);
        let tr = [];
        data.forEach((e, idx) => {
            tr.push(`<tr><td>${e.Company}</td><td class="text-bold">${e.InactivityPeriod}</td><td class="text-success text-bold">${e.CaptureFrecuency}</td><td>${e.UploadFrecuency}</td><td>${e.LocationFrecuency}</td><td>${e.DateCreation}</td><td><a class="btn btn-outline-secondary" href="EditParameterSystem?Id= ${e.Id_Configuration}"><i class="fa fa-edit"></i></a></td></tr>`);
        });
        $("#thead").after(`<tbody class="text-center">${tr}</tbody>`);
        _dtTable.init(id);
        _dtTable.dataArray = data;
    }).fail((jqXHR) => {
        console.log(jqXHR);
    });
}


_dtTable.init = (id) => {
    $(id).DataTable();
}

_dtTable.clear = (id) => {
    $(id).dataTable().fnClearTable();
    $(id).dataTable().fnDestroy();
};

_dtTable.dataArray = [];