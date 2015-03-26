var docCookies = {
    getItem: function (sKey) {
        if (!sKey) { return null; }
        return decodeURIComponent(document.cookie.replace(new RegExp("(?:(?:^|.*;)\\s*" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=\\s*([^;]*).*$)|^.*$"), "$1")) || null;
    },
    setItem: function (sKey, sValue, vEnd, sPath, sDomain, bSecure) {
        if (!sKey || /^(?:expires|max\-age|path|domain|secure)$/i.test(sKey)) { return false; }
        var sExpires = "";
        if (vEnd) {
            switch (vEnd.constructor) {
                case Number:
                    sExpires = vEnd === Infinity ? "; expires=Fri, 31 Dec 9999 23:59:59 GMT" : "; max-age=" + vEnd;
                    break;
                case String:
                    sExpires = "; expires=" + vEnd;
                    break;
                case Date:
                    sExpires = "; expires=" + vEnd.toUTCString();
                    break;
            }
        }
        document.cookie = encodeURIComponent(sKey) + "=" + encodeURIComponent(sValue) + sExpires + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "") + (bSecure ? "; secure" : "");
        return true;
    },
    removeItem: function (sKey, sPath, sDomain) {
        if (!this.hasItem(sKey)) { return false; }
        document.cookie = encodeURIComponent(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT" + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "");
        return true;
    },
    hasItem: function (sKey) {
        if (!sKey) { return false; }
        return (new RegExp("(?:^|;\\s*)" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=")).test(document.cookie);
    },
    keys: function () {
        var aKeys = document.cookie.replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, "").split(/\s*(?:\=[^;]*)?;\s*/);
        for (var nLen = aKeys.length, nIdx = 0; nIdx < nLen; nIdx++) { aKeys[nIdx] = decodeURIComponent(aKeys[nIdx]); }
        return aKeys;
    }
};

(function () {

    var that = this;

    /**************************************/
    /* run this function if window resize */
    /**************************************/

    this.widthLess1024 = function () {

        if ($("#sidebar, #navbar").length <= 0) {
            if ($(window).width() < 1024) {
                $("#content").animate({
                    paddingLeft: "0",
                    paddingRight: "0"
                }, 150);

                $("#content .small-form").animate({
                    paddingTop: "0"
                }, 150);
            } else {
                $("#content").animate({
                    paddingLeft: "0",
                    paddingRight: "0"
                }, 150);

                $("#content .small-form").animate({
                    paddingTop: "100px"
                }, 150);
            }

            return;
        }

        if ($(window).width() < 1024) {
            //make sidebar collapsed
            $('#sidebar, #navbar').addClass('collapsed');
            $('#navigation').find('.dropdown.open').removeClass('open');
            $('#navigation').find('.dropdown-menu.animated').removeClass('animated');

            //move content if navigation is collapsed
            if ($('#sidebar').hasClass('collapsed')) {
                $('#content').animate({ left: "0px", paddingLeft: "55px" }, 150);
                $("#content .small-form").animate({ paddingTop: "0" }, 150);
            } else {
                $('#content').animate({ paddingLeft: "55px" }, 150);
                $("#content .small-form").animate({ paddingTop: "0" }, 150);
            };
        } else {
            //make navigation not collapsed
            $('#sidebar, #navbar').removeClass('collapsed');

            //move content if navigation is not collapsed
            if ($('#sidebar').hasClass('collapsed')) {
                $('#content').animate({ left: "210px", paddingLeft: "265px" }, 150);
                $("#content .small-form").animate({ paddingTop: "100px" }, 150);
            } else {
                $('#content').animate({ paddingLeft: "265px" }, 150);
                $("#content .small-form").animate({ paddingTop: "100px" }, 150);
            };
        }

    };

    this.widthLess768 = function () {
        if ($(window).width() < 768) {
            //paste top navbar objects to sidebar
            if ($('.collapsed-content .search').length === 1) {
                $('#main-search').appendTo('.collapsed-content .search');
            }
            if ($('.collapsed-content li.user').length === 0) {
                $(".collapsed-content li.search").after($("#current-user"));
            }
        } else {
            //show content of top navbar
            $('#current-user').show();

            //remove top navbar objects from sidebar
            if ($('.collapsed-content .search').length === 2) {
                $(".nav.refresh").after($("#main-search"));
            }
            if ($('.collapsed-content li.user').length === 1) {
                $(".quick-actions >li:last-child").before($("#current-user"));
            }
        }
    }

    /*****************************/
    /* INITIALIZE MAIN NICESCROLL*/
    /*****************************/

    this.initContentScroll = function () {
        $("#content").niceScroll({
            cursorcolor: '#000000',
            zindex: 999999,
            bouncescroll: true,
            cursoropacitymax: 0.4,
            cursorborder: '',
            cursorborderradius: 7,
            cursorwidth: '7px',
            background: 'rgba(0,0,0,.1)',
            autohidemode: false,
            railpadding: { top: 0, right: 2, left: 2, bottom: 0 }
        });
    };

    /*************************/
    /* BACKGROUND MANAGEMENT */
    /*************************/

    this.assignBackground = function(id) {
    
        if (id.lastIndexOf('video', 0) === 0) {

            $('#video').html('');
            $('body .videocontent').prepend('<div class="video-background"></div>');
            $('.video-background').addClass(id);
            that.changeVideoBg();

            $('#videobg-check').prop('checked', true);

        } else {

            var lastClass = $('body').attr('class').split(' ').pop();
            $('body').removeClass(lastClass).addClass(id);

        }
    
    }

    this.loadBackground = function () {

        var cookie = docCookies.getItem('background');

        if (cookie != null && cookie != '') {

            that.assignBackground(cookie);

        } else {

            $.ajax({
                url: '/Account/GetBackground',
                type: 'GET',
                dataType: 'json',
                cache: 'false',
                success: function (response) {
                    
                    if (response == null || response == '') {
                        return;
                    }

                    that.assignBackground(response);
                    docCookies.setItem('background', response, 365, '/');

                },
                error: function (xhr, ajaxOptions, thrownError) {
                    that.assignBackground('bg-1');
                }

            });

        }

    }

    this.saveBackgroundId = function (id) {

        $.ajax({
            url: '/Account/SetBackground',
            type: 'POST',
            dataType: 'json',
            data: { backgroundId: id }
        });

        docCookies.setItem('background', id, 365, '/');

    }

    this.loadVideoBg = function () {
        $('body .videocontent').prepend('<div class="video-background video-bg-1"></div>');

        $('.video-background').videobackground({
            videoSource: [
                ['/Content/video/backgrounds/1.mp4', 'video/mp4'],
                ['/Content/video/backgrounds/1.webm', 'video/webm'],
                ['/Content/video/backgrounds/1.ogv', 'video/ogg']
            ],
            //poster: 'assets/images/backgrounds/video/1.jpg',
            controlPosition: '#video',
            loop: true,
            controlText: '',
            loadedCallback: function () {
                $(this).videobackground('mute');
            }
        });
    }

    this.changeVideoBg = function () {
        var backgroundNumber = $('.video-background').attr('class').split(' ').pop().split('-').pop();

        $('.video-background').videobackground({
            videoSource: [
                ['/Content/video/backgrounds/' + backgroundNumber + '.mp4', 'video/mp4'],
                ['/Content/video/backgrounds/' + backgroundNumber + '.webm', 'video/webm'],
                ['/Content/video/backgrounds/' + backgroundNumber + '.ogv', 'video/ogg']
            ],
            //poster: 'assets/images/backgrounds/video/' + backgroundNumber + '.jpg',
            controlPosition: '#video',
            loop: true,
            controlText: '',
            loadedCallback: function () {
                $(this).videobackground('mute');
            }
        });
    }

}).call(window.minimal = window.minimal || {});

$(function() {

    /********************/
    /* INITIALIZE MMENU */
    /********************/

    $("#mmenu").mmenu({
        position: "right",
        zposition: 'next',
        moveBackground: false
    });

    /************************************************/
    /* ADD ANIMATION TO TOP MENU & SUBMENU DROPDOWN */
    /************************************************/

    $('.quick-actions .dropdown').on('show.bs.dropdown', function() {
        $(this).find('.dropdown-menu').addClass('animated fadeInDown');
        $(this).find('#user-inbox').addClass('animated bounceIn');
    });

    $('#navigation .dropdown').on('show.bs.dropdown', function() {
        $(this).find('.dropdown-menu').addClass('animated fadeInLeft');
    });

    /*********************************/
    /* INITIALIZE SIDEBAR BAR CHARTS */
    /*********************************/

    $('#sales-chart').sparkline([5, 6, 7, 2, 1, 4, 6, 8, 10, 14], {
        width: '60px',
        type: 'bar',
        height: '40px',
        barWidth: '6px',
        barSpacing: 1,
        barColor: '#d9544f'
    });

    $('#balance-chart').sparkline([5, 6, 7, 2, 1, 4, 6, 8, 10, 14], {
        width: '60px',
        type: 'bar',
        height: '40px',
        barWidth: '6px',
        barSpacing: 1,
        barColor: '#418bca'
    });

    /****************************/
    /* SIDEBAR PARTS COLLAPSING */
    /****************************/

    $('#sidebar .sidebar-toggle').on('click', function() {
        var target = $(this).data('toggle');

        $(target).toggleClass('collapsed');
    });

    /*********************************/
    /* INITIALIZE SIDEBAR NICESCROLL */
    /*********************************/

    $("#sidebar").niceScroll({
        cursorcolor: '#000000',
        zindex: 999999,
        bouncescroll: true,
        cursoropacitymax: 0.4,
        cursorborder: '',
        cursorborderradius: 0,
        cursorwidth: '7px',
        railalign: 'left',
        railoffset: { top: 45, left: 0 }
    });

    minimal.initContentScroll();

    $('#mmenu').on(
        "opened.mm",
        function() {
            $("#content").getNiceScroll().hide();
        }
    );

    $('#mmenu').on(
        "closed.mm",
        function() {
            $("#content").getNiceScroll().show();
        }
    );

    $('.modal')
        .on('show.bs.modal', function() {
            $('body, #content').css({ overflow: 'hidden' });
            $("#content").getNiceScroll().remove();
        })
        .on('hide.bs.modal', function() {
            $('body, #content').css({ overflow: '' });
            initContentScroll();
        });

    /************************************/
    /* SIDEBAR MENU DROPDOWNS FUNCTIONS */
    /************************************/

    $('#navigation .dropdown.open').data('closable', false);

    $('#navigation .menu >.dropdown').on({
        "shown.bs.dropdown": function() {
            $(this).data('closable', false);
            // resize scrollbar
            $("#sidebar").getNiceScroll().resize();
        },
        "click": function(e) {

            $(this).data('closable', true);

            if (!$(this).hasClass('open')) {
                $('li.dropdown.open').removeClass('open');
            }

            if ($('#sidebar').hasClass('collapsed')) {
                // Avoid having the menu to close when clicking
                e.stopPropagation();
            }

            // resize scrollbar
            $("#sidebar").getNiceScroll().resize();

        },
        "hide.bs.dropdown": function() {
            return $(this).data('closable');
            // resize scrollbar
            // $("#sidebar").getNiceScroll().resize();
        }
    });

    /*******************************/
    /* SIDEBAR COLLAPSING FUNCTION */
    /*******************************/

    $('.sidebar-collapse a').on('click', function() {
        // Add or remove class collapsed
        $('#sidebar, #navbar').toggleClass('collapsed');

        $('#navigation').find('.dropdown.open').removeClass('open');
        $('#navigation').find('.dropdown-menu.animated').removeClass('animated');
        $('#sidebar > li.collapsed').removeClass('collapsed');

        if ($('#sidebar').hasClass('collapsed')) {
            if ($(window).width() < 1024) {
                //if width is less than 1024px move content to left 0px
                $('#content').animate({ left: "0px" }, 150);
            } else {
                //if width is not less than 1024px give padding 55px to content
                $('#content').animate({ paddingLeft: "55px" }, 150);
            }

        } else {

            if ($(window).width() < 1024) {
                //if width is less than 1024px move content to left 210px
                $('#content').animate({ left: "210px" }, 150);
            } else {
                //if width is not less than 1024px give padding 265px to content
                $('#content').animate({ paddingLeft: "265px" }, 150);
            }
        }

    });

    /**************************/
    /* SIDEBAR CLASS TOGGLING */
    /**************************/

    $('#navigation .menu li').hover(function() {
        $(this).addClass('hovered');
        $("#sidebar").addClass('open');
    }, function(e) {
        $(e.target).parent().removeClass('hovered');
        $(this).removeClass('hovered');
        $("#sidebar").removeClass('open');
    });

    /**************************************/
    /* run this function after page ready */
    /**************************************/

    minimal.widthLess1024();
    minimal.widthLess768();

    /***************************************/
    /* run this functions if window resize */
    /***************************************/

    $(window).resize(function() {
        minimal.widthLess1024();
        minimal.widthLess768();
    });

    /**************/
    /* ANIMATIONS */
    /**************/

    //animate numbers with class .animate-number with data-value attribute
    $(".animate-number").each(function() {
        var value = $(this).data('value');
        var duration = $(this).data('animation-duration');

        $(this).animateNumbers(value, true, duration, "linear");
    });

    //animate progress bars
    $('.animate-progress-bar').each(function() {
        var progress = $(this).data('percentage');

        $(this).css('width', progress);
    });

    // Initialize card flip
    $('.card.hover').hover(function () {
        $(this).addClass('flip');
    }, function () {
        $(this).removeClass('flip');
    });

    /**********************************/
    /* color scheme changing function */
    /**********************************/

    $('#color-schemes li a').click(function() {
        var scheme = $(this).attr('class');
        minimal.saveBackgroundId(scheme);
        var lastClass = $('body').attr('class').split(' ').pop();

        $('body').removeClass(lastClass).addClass(scheme);

        $('#videobg-check').prop('checked', false);
        $('#video').html('');
    });

    /*******************************/
    /* VIDEO BACKGROUND INITIALIZE */
    /*******************************/

    $('#videobackgrounds li a').click(function() {
        var background = $(this).attr('class');

        minimal.saveBackgroundId(background);

        $('#video').html('');

        $('body .videocontent').prepend('<div class="video-background"></div>');

        $('.video-background').addClass(background);

        minimal.changeVideoBg();

        $('#videobg-check').prop('checked', true);
    });

    if ($('#videobg-check').is(':checked')) {
        minimal.loadVideoBg();
    }

    $('#videobg-check').change(function() {
        if ($(this).is(":checked")) {
            minimal.loadVideoBg();
        } else {
            $('#video').html('');
            $(this).prop('checked', false);
        }
    });

    /*************************/
    /* page refresh function */
    /************************/

    $('.page-refresh').click(function() {
        location.reload();
    });

    /**************************/
    /* block element function */
    /**************************/

    function elBlock(el) {
        $(el).block({
            message: '<div class="el-reloader"></div>',
            overlayCSS: {
                opacity: 0.6,
                cursor: 'wait',
                backgroundColor: '#000000'
            },
            css: {
                padding: '5px',
                border: '0',
                backgroundColor: 'transparent'
            }
        });
    };

    /****************************/
    /* unblock element function */
    /****************************/

    function elUnblock(el) {
        $(el).unblock();
    };

    /*************************/
    /* tile refresh function */
    /*************************/

    $('.tile-header .controls .refresh').click(function() {
        var el = $(this).parent().parent().parent();
        elBlock(el);
        window.setTimeout(function() {
            elUnblock(el);
        }, 1000);

        return false;
    });

    /************************/
    /* tile remove function */
    /************************/

    $('.tile-header .controls .remove').click(function() {
        $(this).parent().parent().parent().addClass('animated fadeOut');
        $(this).parent().parent().parent().attr('id', 'el_remove');
        setTimeout(function() {
            $('#el_remove').remove();
        }, 500);

        return false;
    });

    /************************/
    /* tile minimize function */
    /************************/

    $('.tile-header .controls .minimize').click(function() {
        $(this).parent().parent().parent().toggleClass('minimized');

        return false;
    });

    // This entire section makes Bootstrap Modals work with iOS
    if (navigator.userAgent.match(/iPhone|iPad|iPod/i)) {

        $('.modal').on('show.bs.modal', function() {
            setTimeout(function() {
                var scrollLocation = $(window).scrollTop();
                $('.modal')
                    .addClass('modal-ios')
                    .height($(window).height())
                    .css({ 'margin-top': scrollLocation + 'px' });
            }, 0);
        });

        $('input, textarea').on('blur', function() {
            setTimeout(function() {
                // This causes iOS to refresh, fixes problems when virtual keyboard closes
                $(window).scrollLeft(0);

                var $focused = $(':focus');
                // Needed in case user clicks directly from one input to another
                if (!$focused.is('input')) {
                    // Otherwise reset the scoll to the top of the modal
                    $(window).scrollTop(scrollLocation);
                }
            }, 0);
        });

    }

});

/******************/
/* page preloader */
/******************/
$(window).load(function () {

    $("#loader").delay(500).fadeOut(300);
    $(".mask").delay(800).fadeOut(300, function () {
        minimal.widthLess1024();
        minimal.widthLess768();
    });

    minimal.loadBackground();

});