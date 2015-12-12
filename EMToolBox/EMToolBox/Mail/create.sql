CREATE DATABASE EMMAIL;
USE EMMAIL;

CREATE TABLE PATTERN(
	ID int identity(1,1),
	NAME varchar(255) not null,
	CONTENT varchar(255) not null,
	HTML bit not null,
	CONSTRAINT [PK_PATTERN] PRIMARY KEY CLUSTERED ([ID] ASC),
	CONSTRAINT [IX_PATTERN] UNIQUE NONCLUSTERED ([NAME] ASC)
);

CREATE TABLE QUEUE(
	ID int identity(1,1),
	PATTERN_ID int not null,
	[TO] varchar(255) not null,
	SUBJECT varchar(255) not null,
	BODY varchar(255) not null,
	CREATIONDATE datetime2(7) null,
	SENDDATE datetime2(7) null,
	SEND bit not null DEFAULT 0,
	CONSTRAINT [PK_QUEUE] PRIMARY KEY CLUSTERED ([ID] ASC),
	FOREIGN KEY (PATTERN_ID) REFERENCES PATTERN(ID)
);

CREATE TABLE SERVER(
	ID int identity(1,1),
	IP varchar(255) not null,
	LOGIN varchar(255) not null,
	PASSWORD varchar(255) null,
	PORT int not null,
	SSL bit not null,
	ENABLE bit not null DEFAULT 1,
	CONSTRAINT [PK_SERVER] PRIMARY KEY CLUSTERED ([ID] ASC)
);


UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px;">
            Afin de valider votre inscription, merci de cliquer sur le lien suivant:
            <a href="http://lbcalerter.com/Account/RegisterConfirmation/{Token}">
                http://lbcalerter.com/Account/RegisterConfirmation
            </a>
        </div>
        <div style="text-align: center; font-size: 10px;">
            Vous recevez cet email car vous êtes inscrit sur
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_CONFIRMATION'

UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px;">
            Afin de réinitialiser votre mot de passe, merci de cliquer sur le lien suivant:
            <a href="http://lbcalerter.com/Account/ResetPasswordConfirmation/{Token}">
                http://lbcalerter.com/Account/ResetPasswordConfirmation
            </a>
        </div>
        <div style="text-align: center; font-size: 10px;">
            Vous recevez cet email car vous êtes inscrit sur
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_RESET'

-- Pattern de base

UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px;">
			Mise en ligne le {Date}<br />
			{Contents>Place}<br />
			{Contents>Price}<br /><br />
            <a href="{Url}" target="_blank">
                <img src="{Contents>PictureUrl}" alt="{Title}" style="width:160px; height: 120px;" />
            </a>
        </div>
        <div style="text-align: center; font-size: 10px;">Vous recevez cet email car vous êtes inscrit sur 
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches, 
			ou cliquez <a href="http://lbcalerter.com/Search/Disable?id={SearchId}&adId={Id}" target="_blank">ici</a> pour désactiver cette alerte.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_AD'

UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px; ">
            Nombre d''annonces: {AdCount}<br />
            Nombre de recherches: {AttemptCount} (1 toutes les {AttemptCadence} mins)<br /><br />
            Liste des annonces du jour:
            {Ads}
        </div>
        <div style="text-align: center; font-size: 10px;">
            Vous recevez cet email car vous êtes inscrit sur
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches, 
			ou cliquez <a href="http://lbcalerter.com/Search/Disable?id={Id}&adId={AdId}" target="_blank">ici</a> pour désactiver cette alerte.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_RECAP'

UPDATE PATTERN SET CONTENT = '<span style="font-weight: bold; display:block; padding: 20px;">{Title}</span><br />
Mise en ligne le {Date}<br />
{Contents>Place}<br />
{Contents>Price}<br /><br />
<a href="{Url}" target="_blank">
    <img src="{Contents>PictureUrl}" alt="{Title}" style="width:160px; height: 120px;" />
</a>'
WHERE NAME = 'LBC_RECAP_AD'

-- Pattern complet

UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px;">
			Mise en ligne le {Date}, par <a href="{Contents>ContactUrl}">{Contents>Name}</a> [Contents>Phone](<img src="{Contents>Phone}" alt="telephone"/>)[/Contents>Phone]<br /><br />
            <a href="{Url}" target="_blank">
                #Contents>PictureUrl#<img src="{Contents>PictureUrl}" alt="{Title}" style="width:160px; height: 120px;" />#/Contents>PictureUrl#
            </a><br />
			#Contents>Param#{Contents>Param}<br />#/Contents>Param#<br />
			[Contents>Latitude]<a href="http://maps.google.com/maps?q={Contents>Latitude},{Contents>Longitude}" target="_blank">Voir sur une carte</a><br /><br />[/Contents>Latitude]
			Description:<br />
			{Contents>Description}
        </div>
        <div style="text-align: center; font-size: 10px;">Vous recevez cet email car vous êtes inscrit sur 
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches, 
			ou cliquez <a href="http://lbcalerter.com/Search/Disable?id={SearchId}&adId={Id}" target="_blank">ici</a> pour désactiver cette alerte.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_AD_FULL'

UPDATE PATTERN SET CONTENT = '<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>{Title}</title>
    <style type="text/css">
        @import url(http://fonts.googleapis.com/css?family=Lato);
    </style>
</head>
<body style="font-family: Lato, sans-serif; font-weight: 500;">
    <div style="width: 600px; margin: 0 auto;">
        <div style="background-color: #0073a3; border-top-left-radius: 4px; border-top-right-radius: 4px; padding: 20px; color: #FFF; font-size: 20px;">
			{Title}
		</div>
        <div style="border: solid #ccc; border-width: 0 1px 1px 1px; border-bottom-left-radius: 4px; border-bottom-right-radius: 4px; padding: 20px; font-size: 16px; ">
            Nombre d''annonces: {AdCount}<br />
            Nombre de recherches: {AttemptCount} (1 toutes les {AttemptCadence} mins)<br /><br />
            Liste des annonces du jour:
            {Ads}
        </div>
        <div style="text-align: center; font-size: 10px;">
            Vous recevez cet email car vous êtes inscrit sur
            <a href="http://lbcalerter.com" target="_blank">LBCAlerter</a>.<br />
            Si vous recevez trop d''emails, connectez-vous à votre espace pour modifier vos recherches, 
			ou cliquez <a href="http://lbcalerter.com/Search/Disable?id={Id}&adId={AdId}" target="_blank">ici</a> pour désactiver cette alerte.
        </div>
    </div>
</body>
</html>'
WHERE NAME = 'LBC_RECAP_FULL'

UPDATE PATTERN SET CONTENT = '<span style="font-weight: bold; display:block; padding: 20px;">{Title}</span><br />
Mise en ligne le {Date}, par <a href="{Contents>ContactUrl}">{Contents>Name}</a> [Contents>Phone](<img src="{Contents>Phone}" alt="telephone"/>)[/Contents>Phone]<br /><br />
<a href="{Url}" target="_blank">
    #Contents>PictureUrl#<img src="{Contents>PictureUrl}" alt="{Title}" style="width:160px; height: 120px;" />#/Contents>PictureUrl#
</a><br />
#Contents>Param#{Contents>Param}<br />#/Contents>Param#<br />
[Contents>Latitude]<a href="http://maps.google.com/maps?q={Contents>Latitude},{Contents>Longitude}" target="_blank">Voir sur une carte</a><br /><br />[/Contents>Latitude]
Description:<br />
{Contents>Description}'
WHERE NAME = 'LBC_RECAP_AD_FULL'
