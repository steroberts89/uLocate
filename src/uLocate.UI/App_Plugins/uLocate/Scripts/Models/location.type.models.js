﻿(function (models, undefined) {

    models.LocationTypeProperty = function(data) {
        var self = this;
        if (data === undefined) {
            self.isDefaultProp = false;
            self.key = '00000000-0000-0000-0000-000000000000';
            self.propAlias = '';
            self.propName = '';
            self.propType = '';
            self.sortOrder = 0;
        } else {
            self.isDefaultProp = data.isDefaultProp;
            self.key = data.key;
            self.propAlias = data.propAlias;
            self.propName = data.propName;
            self.propType = data.propType;
            self.sortOrder = data.sortOrder;
        }
    }

    models.LocationType = function (data) {
        var self = this;
        if (data === undefined) {
            self.description = '';
            self.icon = '';
            self.key = '00000000-0000-0000-0000-000000000000';
            self.name = '';
            self.properties = [new uLocate.Models.LocationTypeProperty()];
        } else {
            if ((typeof data.description) != 'object') {
                self.description = data.description;
            } else {
                self.description = '';
            }
            self.icon = data.icon;
            self.key = data.key;
            self.name = data.name;
            if (data.properties) {
                self.properties = _.map(data.properties, function(property) {
                    return new uLocate.Models.LocationTypeProperty(property);
                });
                _.sortBy(self.properties, function(property) {
                    return property.sortOrder;
                });
            } else {
                self.properties = [];
            }
        }
    };

}(window.uLocate.Models = window.uLocate.Models || {}));