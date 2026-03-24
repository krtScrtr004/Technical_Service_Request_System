$(".PSGC_ProvinceId").select2();
$(".PSGC_CityMunicipalityId").select2();

$('.req').prop('required', true);
$('#PSGC_ProvinceId').change(function () {
    var provinceId = $('#PSGC_ProvinceId option:selected').val();

    if (provinceId == null) {
        $('#PSGC_CityMunicipalityId').empty();
        document.getElementById("PSGC_CityMunicipalityId").disabled = true;
    } else {
        $('#PSGC_CityMunicipalityId').prop('required', true);
        $.getJSON("@Url.Action("getTown", "Town")", { id: provinceId }, function (data) {
            $('#PSGC_CityMunicipalityId').empty();
            var town = "<option value=\"" + null + "\">-=Select City/Municipality=-</option>";

            $.each(data.list, function (index, da) {
                town += "<option value=\"" + da.Id + "\">" + da.PSGC_CityMunicipalityName + "</option>";
            });
            $('#PSGC_CityMunicipalityId').append(town);

            document.getElementById("PSGC_CityMunicipalityId").disabled = false;
        });
    }
});