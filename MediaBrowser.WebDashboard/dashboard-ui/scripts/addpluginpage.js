﻿(function ($, document, window) {

    function populateHistory(packageInfo, page) {

        var html = '';

        for (var i = 0, length = Math.min(packageInfo.versions.length, 10) ; i < length; i++) {

            var version = packageInfo.versions[i];

            html += '<h2 style="margin:.5em 0;">' + version.versionStr + ' (' + version.classification + ')</h2>';

            html += '<div style="margin-bottom:1.5em;">' + version.description + '</div>';
        }

        $('#revisionHistory', page).html(html);
    }

    function populateVersions(packageInfo, page, installedPlugin) {

        var html = '';

        for (var i = 0, length = packageInfo.versions.length; i < length; i++) {

            var version = packageInfo.versions[i];

            html += '<option value="' + version.versionStr + '|' + version.classification + '">' + version.versionStr + ' (' + version.classification + ')</option>';

        }

        var selectmenu = $('#selectVersion', page).html(html);

        if (!installedPlugin) {

            $('#pCurrentVersion', page).hide().html("");
        }

        var packageVersion = packageInfo.versions.filter(function (current) {

            return current.classification == "Release";
        })[0];

        // If we still don't have a package version to select, pick the first Beta build
        if (!packageVersion) {

            packageVersion = packageInfo.versions.filter(function (current) {

                return current.classification == "Beta";
            })[0];
        }

        if (packageVersion) {
            var val = packageVersion.versionStr + '|' + packageVersion.classification;

            selectmenu.val(val);
        }
    }

    function populateReviews(id, page) {

        ApiClient.getPackageReviews(id, null, null, 3).then(function (positive) {

            var html = '';

            if (positive && positive.length > 0) {

                html += '<div data-role="collapsible" data-collapsed="true" style="margin-top: 2em;" >';
                html += '<h3>' + Globalize.translate('HeaderLatestReviews') + '</h3>';

                html += "<div><br/>";

                for (var i = 0; i < positive.length; i++) {
                    var review = positive[i];

                    html += "<div>";
                    html += "<span class='storeItemReviewText'>";
                    html += new Date(review.timestamp).toDateString();
                    html += " " + RatingHelpers.getStoreRatingHtml(review.rating, review.id, review.name, true);
                    html += " " + review.title;
                    html += "</span>";
                    if (review.review) {
                        html += "<p class='storeItemReviewText'>";
                        html += review.review;
                        html += "</p>";
                    }

                    html += "</div>";
                    html += "<hr/>";
                }
                html += "</div>";
                html += "</div>";
            }

            $('#latestReviews', page).html(html).trigger('create');
        });
    }

    function renderPackage(pkg, installedPlugins, pluginSecurityInfo, page) {

        var installedPlugin = installedPlugins.filter(function (ip) {
            return ip.Name == pkg.name;
        })[0];

        populateVersions(pkg, page, installedPlugin);
        populateHistory(pkg, page);
        if (pkg.totalRatings > 0) populateReviews(pkg.id, page);

        $('.pluginName', page).html(pkg.name);

        if (pkg.targetSystem == 'Server') {
            $("#btnInstallDiv", page).removeClass('hide');
            $("#nonServerMsg", page).hide();
            $("#pSelectVersion", page).removeClass('hide');
        } else {
            $("#btnInstallDiv", page).addClass('hide');
            $("#pSelectVersion", page).addClass('hide');

            var msg = Globalize.translate('MessageInstallPluginFromApp');
            $("#nonServerMsg", page).html(msg).show();
        }

        if (pkg.shortDescription) {
            $('#tagline', page).show().html(pkg.shortDescription);
        } else {
            $('#tagline', page).hide();
        }

        $('#overview', page).html(pkg.overview || "");


        $('#developer', page).html(pkg.owner);

        RegistrationServices.renderPluginInfo(page, pkg, pluginSecurityInfo);

        //Ratings and Reviews
        var ratingHtml = RatingHelpers.getStoreRatingHtml(pkg.avgRating, pkg.id, pkg.name);
        ratingHtml += "<span class='storeReviewCount'>";
        ratingHtml += " " + Globalize.translate('ValueReviewCount').replace('{0}', pkg.totalRatings);
        ratingHtml += "</span>";

        $('#ratingLine', page).html(ratingHtml);

        if (pkg.richDescUrl) {
            $('#pViewWebsite', page).show();
            $('#pViewWebsite a', page).attr('href', pkg.richDescUrl);
        } else {
            $('#pViewWebsite', page).hide();
        }

        if (pkg.previewImage || pkg.thumbImage) {

            var color = pkg.tileColor || "#38c";
            var img = pkg.previewImage ? pkg.previewImage : pkg.thumbImage;
            $('#pPreviewImage', page).show().html("<img src='" + img + "' style='max-width: 100%;-moz-box-shadow: 0 0 20px 3px " + color + ";-webkit-box-shadow: 0 0 20px 3px " + color + ";box-shadow: 0 0 20px 3px " + color + ";' />");
        } else {
            $('#pPreviewImage', page).hide().html("");
        }

        if (installedPlugin) {

            var currentVersionText = Globalize.translate('MessageYouHaveVersionInstalled').replace('{0}', '<strong>' + installedPlugin.Version + '</strong>');
            $('#pCurrentVersion', page).show().html(currentVersionText);

        } else {
            $('#pCurrentVersion', page).hide().html("");
        }

        Dashboard.hideLoadingMsg();
    }

    $(document).on('pageinit', "#addPluginPage", function () {

        $('.addPluginForm').off('submit', AddPluginPage.onSubmit).on('submit', AddPluginPage.onSubmit);

    }).on('pageshow', "#addPluginPage", function () {

        var page = this;

        Dashboard.showLoadingMsg();

        var name = getParameterByName('name');
        var guid = getParameterByName('guid');

        var promise1 = ApiClient.getPackageInfo(name, guid);
        var promise2 = ApiClient.getInstalledPlugins();
        var promise3 = ApiClient.getPluginSecurityInfo();

        Promise.all([promise1, promise2, promise3]).then(function (responses) {

            renderPackage(responses[0], responses[1], responses[2], page);

        });

    }).on('pagebeforeshow pageinit pageshow', "#addPluginPage", function () {

        // This needs both events for the helpurl to get done at the right time

        var page = this;

        var context = getParameterByName('context');

        $('.syncTabs', page).hide();
        $('.pluginTabs', page).hide();
        $('.livetvTabs', page).hide();
        $('.notificationsTabs', page).hide();

        if (context == 'sync') {
            $('.syncTabs', page).show();

            page.setAttribute('data-helpurl', 'https://github.com/MediaBrowser/Wiki/wiki/Sync');
            Dashboard.setPageTitle(Globalize.translate('TitleSync'));
        }
        else if (context == 'livetv') {

            $('.livetvTabs', page).show();

            Dashboard.setPageTitle(Globalize.translate('TitleLiveTV'));
            page.setAttribute('data-helpurl', 'https://github.com/MediaBrowser/Wiki/wiki/Live%20TV');
        }
        else if (context == 'notifications') {

            $('.notificationsTabs', page).show();

            Dashboard.setPageTitle(Globalize.translate('TitleNotifications'));
            page.setAttribute('data-helpurl', 'https://github.com/MediaBrowser/Wiki/wiki/Notifications');
        }
        else {
            $('.pluginTabs', page).show();

            page.setAttribute('data-helpurl', 'https://github.com/MediaBrowser/Wiki/wiki/Plugins');
            Dashboard.setPageTitle(Globalize.translate('TitlePlugins'));
        }

    });

    function performInstallation(page, packageName, guid, updateClass, version) {

        var developer = $('#developer', page).html().toLowerCase();

        var alertCallback = function (confirmed) {

            if (confirmed) {

                Dashboard.showLoadingMsg();

                ApiClient.installPlugin(packageName, guid, updateClass, version).then(function () {

                    Dashboard.hideLoadingMsg();
                });
            }
        };

        if (developer != 'luke' && developer != 'ebr') {

            Dashboard.hideLoadingMsg();

            var msg = Globalize.translate('MessagePluginInstallDisclaimer');
            msg += '<br/>';
            msg += '<br/>';
            msg += Globalize.translate('PleaseConfirmPluginInstallation');

            require(['confirm'], function (confirm) {

                confirm(msg, Globalize.translate('HeaderConfirmPluginInstallation')).then(function () {

                    alertCallback(true);
                }, function () {

                    alertCallback(false);
                });

            });

        } else {
            alertCallback(true);
        }
    }

    function addPluginpage() {

        var self = this;

        self.onSubmit = function () {

            Dashboard.showLoadingMsg();

            var page = $(this).parents('#addPluginPage')[0];

            page.querySelector('#btnInstall').disabled = true;

            var name = getParameterByName('name');
            var guid = getParameterByName('guid');

            ApiClient.getInstalledPlugins().then(function (plugins) {

                var installedPlugin = plugins.filter(function (ip) {
                    return ip.Name == name;
                })[0];

                var vals = $('#selectVersion', page).val().split('|');

                var version = vals[0];

                if (installedPlugin && installedPlugin.Version == version) {

                    Dashboard.hideLoadingMsg();

                    Dashboard.alert({
                        message: Globalize.translate('MessageAlreadyInstalled'),
                        title: Globalize.translate('HeaderPluginInstallation')
                    });
                } else {
                    performInstallation(page, name, guid, vals[1], version);
                }
            });

            return false;
        };
    }

    window.AddPluginPage = new addPluginpage();

})(jQuery, document, window);