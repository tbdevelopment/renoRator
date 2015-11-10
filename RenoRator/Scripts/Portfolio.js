$(function () {

    $('#portfolioJobsHolder').css('width', $('.portfolioJob').length * 118);
    $('.portfolioJob').click(function () {
        var clickedID = $(this).attr('data-id');
        var jobs = [];
        $('.portfolioJob').each(function () {
            jobs.push($(this)[0].outerHTML);
        });
        $('#portfolioJobs').html(jobs.join(''));

        $('#portfolioJobs .portfolioJob[data-id=' + clickedID + ']').addClass('activePicture');

        $('#portfolioJobs .portfolioJob').addClass('dialogJob').removeClass('portfolioJob').click(function () {
            getJob($(this).attr('data-id'));
            $('.activePicture').removeClass('activePicture');
            $(this).addClass('activePicture');
        });
        clearSelectedJob();
        $('#portfoliowrapper').dialog({
            modal:true,
            width: 960,
            height: 600,
            open: function () {
                getJob(clickedID);
            }
        });
    });
});

function getJob(jobID) {
    $.ajax({
        url: "/Portfolio/getPortfolioJobByID",
        data: '{ "jobID": ' + jobID + ' }',
        type: "POST",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            showJob(data);
        }
    });
}

function showJob(job) {
    $('#jobTitle').html(job.title);
    $('#jobAddress').html(job.addressLine1 + "<br />" + job.addressLine2 + "<br />" + job.city + ", " + job.province + "<br />" + job.postalCode);
    $('#jobContractor').html(job.contractor);

    if (job.startDate != undefined && job.startDate != null)
        $('#jobStartDate').html(formatDate(new Date(parseInt(job.startDate.substr(6)))));
    $('#jobEndDate').html(formatDate(new Date(parseInt(job.endDate.substr(6)))));
    $('#jobDescription').html(job.description);
    $('#jobPriceRange').html(job.minPriceRange + ' - ' + job.maxPriceRange);
    $('#tags').html(job.tags.replace('|', ', '));

    var newPhotos = '';
    $.each(job.photos, function () {
        newPhotos += '<li><a class="thumb" href="' + this.path + '" name="leaf">';
        newPhotos += '<img data-id="' + this.photoID + '" src="' + this.thumbPath + '" />';
        newPhotos += '</a></li>';
        
    });
    $('.thumbsContainer ul.thumbs').html(newPhotos);
    initializeAdvancedGallery();
    $('#selectedJob').show();
}

function formatDate(d) {
    var dateArr = [];
    dateArr.push(d.getFullYear());
    dateArr.push(d.getMonth());
    dateArr.push(d.getDate());
    return dateArr.join('/');
}

function clearSelectedJob() {
    var html = '<div class="jobDesc"><div class="gallerificContainer"><div class="gallery content"><div class="controls"></div><div class="slideshow-container"><div id="loading" class="loader"></div><div id="slideshow" class="slideshow"></div></div></div><div class="thumbsContainer"><ul class="thumbs noscript"></ul></div></div>';
    html += '<div class="InfoContainer"><div id="jobTitle" class="jobTitle"></div><div><table><tbody><tr><td class="alignTop"><label>Address:</label></td><td><span id="jobAddress"></span></td></tr>';
    html += '<tr><td class="alignTop"><label>Contractor:</label></td><td><span id="jobContractor"></span></td></tr><tr><td class="alignTop"><label>Start Date:</label></td><td><span id="jobStartDate"></span></td></tr>';
    html += '<tr><td class="alignTop"><label>End Date:</label></td><td><span id="jobEndDate"></span></td></tr><tr><td class="alignTop"><label>Price Range:</label></td><td><span id="jobPriceRange"></span></td></tr>';
    html += '<tr><td class="alignTop"><label>Tags:</label></td><td><span id="jobTags"></span></td></tr><tr><td class="alignTop"><label>Description:</label></td><td><p id="jobDescription"></p></td></tr></tbody></table></div></div>';
    $('#selectedJob').html(html);
    $('#selectedJob').hide();
}

function initializeAdvancedGallery() {
    var onMouseOutOpacity = 0.67;
    $('.thumbsContainer > ul > li').opacityrollover({
        mouseOutOpacity: onMouseOutOpacity,
        mouseOverOpacity: 1.0,
        fadeSpeed: 'fast',
        exemptionSelector: '.selected'
    });

    // Initialize Advanced Galleriffic Gallery
    $('.thumbsContainer').galleriffic({
        delay: 2500,
        numThumbs: 4,
        preloadAhead: 10,
        enableTopPager: true,
        enableBottomPager: false,
        maxPagesToShow: 7,
        imageContainerSel: '.slideshow',
        controlsContainerSel: '.controls',
        //captionContainerSel: '#caption',
        loadingContainerSel: '.loader',
        renderSSControls: true,
        renderNavControls: true,
        playLinkText: 'Play',
        pauseLinkText: 'Pause',
        prevLinkText: '&lsaquo;Prev',
        nextLinkText: 'Next&rsaquo;',
        nextPageLinkText: 'Next &rsaquo;',
        prevPageLinkText: '&lsaquo; Prev',
        enableHistory: false,
        autoStart: false,
        syncTransitions: true,
        defaultTransitionDuration: 900,
        onSlideChange: function (prevIndex, nextIndex) {
            // 'this' refers to the gallery, which is an extension of $('#thumbs')
            this.find('ul').children()
                    .eq(prevIndex).fadeTo('fast', onMouseOutOpacity).end()
                    .eq(nextIndex).fadeTo('fast', 1.0);
        },
        onPageTransitionOut: function (callback) {
            this.fadeTo('fast', 0.0, callback);
        },
        onPageTransitionIn: function () {
            this.fadeTo('fast', 1.0);
        }
    });
}