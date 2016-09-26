<?php
require 'vendor/autoload.php';

use Gui\Application;
use Gui\Components\Label;
use Gui\Output;
use Curl\Curl;


$application = new Application([
    'title' => 'MiFi Status',
    'width' => 384,
    'height' => 100,
    'icon' => __DIR__.'/wifi.ico',
]);

$application->on('start', function() use ($application) {

    $application->setVerboseLevel(0);

    new Label([
        'text' => "Connection",
        'fontSize' => 10,
        'top' => 5,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);
    new Label([
        'text' => "Network",
        'fontSize' => 10,
        'top' => 20,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);
    new Label([
        'text' => "SIM",
        'fontSize' => 10,
        'top' => 35,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);
    new Label([
        'text' => "Usage",
        'fontSize' => 10,
        'top' => 50,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);
    new Label([
        'text' => "Battery",
        'fontSize' => 10,
        'top' => 65,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);
    new Label([
        'text' => "Speed (up/down)",
        'fontSize' => 10,
        'top' => 80,
        'fontFamily' => 'Source Code Pro',
        'left' => 5,
    ]);

    $connectionLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 5,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $networkLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 20,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $simLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 35,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $usageLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 50,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $batteryLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 65,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $speedLabel = new Label([
        'text' => "-",
        'fontSize' => 10,
        'top' => 80,
        'fontFamily' => 'Source Code Pro',
        'left' => 144,
    ]);
    $application->loop->addPeriodicTimer(5, function() use($connectionLabel, $networkLabel, $simLabel, $usageLabel, $batteryLabel, $speedLabel) {

        $curl = new Curl();
        $curl->setTimeout(1);
        $curl->post('http://192.168.0.1/cgi-bin/qcmap_web_cgi', json_encode([
            'module' => 'status',
            'action' => 0,
        ]));


        $status = [
            'connection' => ['disable','disconnected','connecting','disconnecting','connected'],
            'network' => ['no service','GSM','WCDMA','LTE','TD-SCDMA','CDMA 1x','CDMA EVDO'],
            'sim' => ['invalid','no SIM','error','ready','PIN requested','PIN verified','PUK requested','permanently locked'],
        ];

        $data = json_decode($curl->response);

        $connectionLabel->setText($status['connection'][$data->wan->connectStatus]);
        $networkLabel->setText($status['network'][$data->wan->networkType]);
        $simLabel->setText($status['sim'][$data->wan->simStatus]);
        $usageLabel->setText(getFilesizeHuman($data->wan->totalStatistics));
        $batteryLabel->setText($data->battery->voltage.'%');
        $speedLabel->setText(getFilesizeHuman($data->wan->txSpeed).' / '.getFilesizeHuman($data->wan->rxSpeed));
    });
});

$application->run();

function getFilesizeHuman(int $bytes, int $decimals = 2) : string
{
    $suffixes = 'BKMGTP';
    $factor = floor((strlen((string) $bytes) - 1) / 3);

    return sprintf("%.{$decimals}f", $bytes / pow(1024, $factor)).' '.@$suffixes[$factor].(@$suffixes[$factor] !== 'B' ? 'B' : '');
}