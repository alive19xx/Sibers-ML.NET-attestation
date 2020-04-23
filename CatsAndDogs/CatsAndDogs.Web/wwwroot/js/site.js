// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ResultViewModel() {
    var self = this;
    self.Prediction = ko.observable(null);
    self.Scores = ko.observableArray([]);
    self.Base64 = ko.observable(null);
    self.Show = ko.observable(false);
    self.Clear = function () {
        self.Prediction(null);
        self.Scores([]);
        self.Base64(null);
        self.Show(false);
    }
}
function TestViewModel() {
    var self = this;
    var nextId = 0;
    var allLoaded = false;
    self.items = ko.observableArray([]);
    self.loadNext = function () {
        if (allLoaded)
            return;
        $.ajax({
            url: `test/next/${nextId}`,
            success: function (response) {
                response.ids.forEach(function (id) {

                    var itemVm = new ItemViewModel();
                    itemVm.id = id;
                    self.items.push(itemVm);
                    $.ajax({
                        url: `/prediction/${itemVm.id}`,
                        type: "POST",
                        success: function (r) {
                            itemVm.prediction(r.predictedLabel);
                            itemVm.base64(r.base64ImageContent);
                            r.scores.forEach(function (el) {
                                itemVm.scores.push(el);
                            });
                            itemVm.isLoaded(true);
                        }
                    })
                    debugger;
                })
                if (!response.nextId) {
                    allLoaded = true;
                }
                else {
                    nextId = response.nextId
                    self.loadNext();
                }
            }
        });
    }

    function ItemViewModel() {
        this.id = null;
        this.isLoaded = ko.observable(false);
        this.scores = ko.observableArray([]);
        this.base64 = ko.observable(null);
        this.prediction = ko.observable(null);
    }
}


function LoadNext(lastId) {
    $.get(`/test/next/${lastId}`).then(function (response) {
        var vm = ResultViewModel();
    });
}

function PredictTest(id, vm) {
    $.post(`/prediction/${id}`).then(function (response) {
        vm.Prediction(response.PredictedLabel);
        vm.Base64(response.Base64ImageContent);
        response.Scores.forEach(function (el) {
            vm.Scores.push(el);
        });
        vm.Show(true);
    });
}

function PredictFile() {
    vm = window.viewModel;
    vm.Clear();
    var formData = new FormData();
    formData.append('image', $('#inputFile')[0].files[0])
    $.ajax({
        url: "prediction",
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {

            vm.Prediction(response.predictedLabel);
            vm.Base64(response.base64ImageContent);
            response.scores.forEach(function (el) {
                vm.Scores.push(el);
            });
            vm.Show(true);
        }
    })

}