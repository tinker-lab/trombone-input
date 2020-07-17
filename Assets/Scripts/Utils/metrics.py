import csv
import time
from collections import defaultdict
from enum import Enum
from math import ceil, sqrt, trunc

import yaml
import matplotlib.pyplot as plt
import numpy as np


class Layouts(Enum):
    SliderOnly = "SliderOnly"
    ArcType = "ArcType"
    TiltType = "TiltType"
    Raycast = "Raycast"


trial_files = {0: "trial-2020-07-16_02-36-27.yaml", 1: "trial-2020-07-16_02-46-22.yaml",
               2: "trial-2020-07-16_03-13-40.yaml", 3: "trial-2020-07-16_03-36-35.yaml"}


def get_all_trials():
    return {v: get_trial(k) for k, v in trial_files.items()}


def get_trial(n=0):
    with open("../../Results/" + trial_files[n]) as yams:
        return yaml.load(yams, Loader=yaml.UnsafeLoader)


def print_yaml_recur(yams, indent=0):
    if isinstance(yams, list):
        for v in yams:
            print_yaml_recur(v, indent + 1)
    elif not isinstance(yams, dict):
        print("  " * indent, yams)
    else:
        for k, v in yams.items():
            print("  " * indent, k, ': ', sep='', end='')
            inline = isinstance(v, list) and all(isinstance(i, float) or isinstance(i, int) for i in v)
            inline = inline or isinstance(v, str) or isinstance(v, float) or isinstance(v, int)
            if inline:
                print(v)
            else:
                print()
                print_yaml_recur(v, indent + 1)


def randrange(n, vmin, vmax):
    return (vmax - vmin) * np.random.rand(n) + vmin


def extract_layout_positions(data: dict, layout, use_practice=False):
    out = list()
    for item in data["trial"]:
        try:
            challenge = item["challenge"]
            if challenge["layout"] == layout and (not use_practice or not challenge['type'] == 'Practice'):
                for kp in item["challenge"]["keypresses"].values():
                    out.append(kp["pressPos"])
        except KeyError:
            pass
        except AttributeError:
            pass
    return out


def merge_trials(*datas):
    out = list()
    for t in datas:
        out.extend(t["trial"])
    return {"meta": "lost in merge", "trial": out}


arctype_x_range_size = 145 - 45
tilttype_x_range_size = 12 - 5
tilttype_z_range_size = 120 - 30


def tilttype_pos(c: str):
    if c == ' ':
        return 6, 3
    if not c.isalpha():
        raise ValueError()
    delta = (ord(c.lower()) - ord('a'))
    return delta // 4, delta % 4


def raycast_pos(c: str):
    for i, row in enumerate(['QWERTYUIOP', 'ASDFGHJKL;', 'ZXCVBNM,. ']):
        if c.upper() in row:
            return row.find(c), i


def raycast_displacement(p, c, signed=False):
    return ((a - b if signed else abs(a - b))for a, b in zip(raycast_pos(p), raycast_pos(c)))


def arctype_bin(c: str):
    if c == ' ':
        return 7
    if not c.isalpha():
        raise ValueError()
    return (ord(c.lower()) - ord('a')) // 4


def arctype_ideal(prompt: str):
    def get_ideal_travel(p, c):
        bin_delta = abs(arctype_bin(p) - arctype_bin(c))
        ideal_travel_per_bin = arctype_x_range_size / ceil(26 / 4)
        return bin_delta * ideal_travel_per_bin

    ideal = 0
    last = None
    for c in prompt:
        if last is not None:
            ideal += get_ideal_travel(last, c)
        last = c

    return ideal


def tilttype_displacement(p, c, signed=False):
    return ((a - b if signed else abs(a - b)) for a, b in zip(tilttype_pos(p), tilttype_pos(c)))


def tilttype_ideal_tuple(prompt: str):
    def get_ideal_travel(p, c):
        x, z = tilttype_displacement(p, c)
        return x * tilttype_x_range_size / ceil(26 / 4), z * tilttype_z_range_size / 4

    ideal = (0, 0)
    last = None
    for c in prompt:
        if last is not None:
            ideal = (a + b for a, b in zip(ideal, get_ideal_travel(last, c)))
        last = c

    return tuple(ideal)


def tilttype_ideal(prompt: str):
    a, b = tilttype_ideal_tuple(prompt)
    return sqrt(a * a + b * b)


def challenge_rot_travel(challenge):
    out = 0
    for kp in challenge['keypresses'].values():
        try:
            out += sqrt(sum(k * k for k in kp['travel']['rot']))
        except KeyError:
            pass

    return out


def words_per_minute(challenge):
    # start = list(challenge["keypresses"].keys())[0] if challenge["keypresses"] else challenge["time"]["start"]
    interval = challenge["time"]["duration"]
    assert interval > 0
    minutes_of_entry = interval / 60
    output = challenge["output"]
    words_entered = max(0, len(output) - 1) / 5
    return words_entered / minutes_of_entry


def accurate_words_per_minutes(challenge):
    prompt = challenge["prompt"]
    a = sum(1 for a, b in zip(challenge["output"], prompt) if a == b) / len(prompt)
    return a * words_per_minute(challenge)


# https://stackoverflow.com/questions/11686720/is-there-a-numpy-builtin-to-reject-outliers-from-a-list
def reject_outliers(data, m=2):
    return data[abs(data - np.mean(data)) < m * np.std(data)]


def make_point_cloud(data):
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    for layout, marker in zip((e.value for e in Layouts), ['o', '^', '.', 's']):
        posses = data.layout_posses[layout]
        if not posses or layout == Layouts.SliderOnly.value:
            continue

        xs = [v[0] for v in posses]
        ys = [v[1] for v in posses]
        zs = [v[2] for v in posses]
        ax.scatter(xs, zs, ys, marker=marker)

        ax.set_autoscalex_on(False)
        ax.set_xlim([-4.5, 1])
        ax.set_ylim([0, 5.5])
        ax.set_zlim([-1, 4.5])

        ax.set_xlabel('X (ft)')
        ax.set_ylabel('Z (ft)')
        ax.set_zlabel('Y (ft)')

    plt.legend([e.value for e in Layouts if e != Layouts.SliderOnly], loc='center left')
    # plt.title('Stylus Positions in Cave on Keypress by Interface')
    plt.savefig("../../Results/Figures/trnsprnt-pos-cloud.png", transparent=True)
    plt.show()


def make_2d_point_cloud(data, axes):
    fig = plt.figure()
    ax = fig.add_subplot(111)

    for layout, marker in zip((e.value for e in Layouts), ['o', '^', '.', 's']):
        posses = data.layout_posses[layout]
        if not posses or layout == Layouts.SliderOnly.value:
            continue

        xs = [v[axes[0]] for v in posses]
        ys = [v[axes[1]] for v in posses]

        ax.scatter(xs, ys, marker=marker)
        # ax.plot(np.mean(xs), np.mean(ys), marker='H')

        # ax.set_autoscalex_on(False)
        # ax.set_xlim([-1, 1])

        axes_labels = ['X', 'Z', 'Y']
        ax.set_xlabel(axes_labels[axes[0]] + ' (feet)')
        ax.set_ylabel(axes_labels[axes[1]] + ' (feet)')

    plt.legend([e.value for e in Layouts if e != Layouts.SliderOnly], loc='center left')
    # plt.title('Stylus Positions in Cave on Keypress by Interface')
    plt.savefig("../../Results/Figures/trnsprnt-pos-2d-cloud-" + '-'.join(map(str, axes)) + ".png", transparent=True)
    plt.savefig("../../Results/Figures/pos-2d-cloud-" + '-'.join(map(str, axes)) + ".png")
    plt.show()


def make_wpm_bars(data):
    layout_awpm = {k: v for k, v in data.layout_awpm.items() if k != Layouts.SliderOnly.value}
    layout_wpm = {k: v for k, v in data.layout_wpm.items() if k != Layouts.SliderOnly.value}
    awpm_data = layout_awpm.values()
    awpm_means = [np.mean(v) for v in awpm_data]
    wpm_data = layout_wpm.values()
    wpm_means = [np.mean(v) for v in wpm_data]
    awpm_std = [np.std(v) for v in awpm_data]
    wpm_std = [np.std(v) for v in wpm_data]
    ind = np.arange(len(awpm_data))  # the x locations for the groups
    width = 0.35  # the width of the bars: can also be len(x) sequence

    p1 = plt.bar(ind, awpm_means, width, align='center')
    p2 = plt.bar(ind, [a - b for a, b in zip(wpm_means, awpm_means)], width, align='center',
                 bottom=awpm_means, yerr=wpm_std)

    plt.axes().yaxis.grid(True)  # raises an error?

    plt.ylabel('(Accurate) Words per Minute')
    # plt.title('Efficiency')
    plt.xticks(ind, layout_awpm.keys())
    plt.legend((p1[0], p2[0]), ('aWPM', 'WPM'))

    plt.savefig('../../Results/Figures/stacked-wpm-awpm.png')
    plt.show()


def make_pit_bars(data):
    # https://matplotlib.org/3.1.1/gallery/lines_bars_and_markers/bar_stacked.html
    layouts = [Layouts.ArcType.value, Layouts.TiltType.value]
    ideal_means = [np.mean(data.layout_ideal_travel[v]) for v in layouts]
    actual_means = [np.mean(data.layout_actual_travel[v]) for v in layouts]
    ideal_std = [np.std(data.layout_ideal_travel[v]) for v in layouts]
    actual_std = [np.std(data.layout_actual_travel[v]) for v in layouts]
    ind = np.arange(2)  # the x locations for the groups
    width = 0.85  # the width of the bars: can also be len(x) sequence

    p1 = plt.bar(ind, ideal_means, width, yerr=ideal_std, align='center')
    p2 = plt.bar(ind, [a - b for a, b in zip(actual_means, ideal_means)], width, align='center',
                 bottom=ideal_means, yerr=actual_std)

    plt.ylabel('Total Angular Displacement (degrees)')
    # plt.title('Travel by Interface')
    plt.xticks(ind, (Layouts.ArcType.value, Layouts.TiltType.value))
    plt.legend((p1[0], p2[0]), ('Ideal', 'Actual'))

    plt.savefig('../../Results/Figures/travel-by-interface-error-bars.png')
    plt.show()

    pitMeans = [np.mean(data.layout_pit[v]) for v in
                [Layouts.ArcType.value, Layouts.TiltType.value]]
    pitStd = [np.std(data.layout_pit[v]) for v in
              [Layouts.ArcType.value, Layouts.TiltType.value]]
    # ind = np.arange(2)  # the x locations for the groups
    # width = 0.85  # the width of the bars: can also be len(x) sequence

    fig, ax = plt.subplots()
    ax.bar(ind, pitMeans, yerr=pitStd, align='center', alpha=0.5, ecolor='black', capsize=10)
    ax.set_ylabel('PIT (%)')
    plt.xticks(ind, (Layouts.ArcType.value, Layouts.TiltType.value))
    # ax.set_title('PIT by Interface')
    ax.yaxis.grid(True)

    plt.savefig('../../Results/Figures/pit-by-interface-error-bars.png')
    plt.show()


def make_duration_lines(data):
    fig, ax = plt.subplots()
    items = list()
    for layout, durations in data.layout_durations.items():
        if layout != Layouts.SliderOnly.value:
            items.append(plt.plot(np.arange(len(durations)), durations))

    plt.legend(items, (e.value for e in Layouts if e != Layouts.SliderOnly))
    fig.show()


def make_error_pies(data):
    items = list()
    for layout, pairs in data.layout_blind_io.items():
        if layout == Layouts.SliderOnly.value:
            continue
        errors = 0
        off_by_one = 0
        dipped = 0
        for prompt, output in pairs:
            for p, o in zip(prompt, output):
                if layout == Layouts.TiltType.value or layout == Layouts.ArcType.value:
                    a, b = tilttype_displacement(p, o, signed=True)
                else:
                    a, b = raycast_displacement(p, o, signed=True)
                if b == -1:
                    dipped += 1
                if abs(a) + abs(b) == 1:
                    off_by_one += 1
                if p != o:
                    errors += 1
        items.append((layout, 100 * dipped / errors, 100 * off_by_one / errors))

    off_by_one_pcts = [obo for _, _, obo in items]
    dipped_pcts = [dip for _, dip, _ in items]
    ind = np.arange(3)  # the x locations for the groups
    width = 0.5  # the width of the bars: can also be len(x) sequence

    p1 = plt.bar(ind, dipped_pcts, width, align='center')[0]
    p2 = plt.bar(ind, [a - b for a, b in zip(off_by_one_pcts, dipped_pcts)], width, align='center', bottom=dipped_pcts)[0]

    plt.ylabel('Percent of Errors')
    # plt.title('Travel by Interface')
    plt.xticks(ind, (e for e in data.layout_blind_io.keys() if e != Layouts.SliderOnly.value))
    plt.xlabel("Layouts")
    plt.legend((p1, p2), ('Dipped (y + 1)', 'Off-by-One'))

    plt.savefig('../../Results/Figures/error-chart.png')
    plt.show()


def get_data(skip_practice=True):
    layout_pit, layout_wpm, layout_awpm = defaultdict(list), defaultdict(list), defaultdict(list)
    layout_actual_travel, layout_ideal_travel, layout_durations = dict(), dict(), defaultdict(list)
    layout_blind_io = defaultdict(list)
    trials = get_all_trials()
    for trial in trials.values():
        for challenge in trial["trial"]:
            if "command" in challenge:
                continue
            else:
                challenge = challenge["challenge"]

            if challenge['type'] == 'Practice' and skip_practice:
                continue

            layout_wpm[challenge["layout"]].append(words_per_minute(challenge))
            layout_awpm[challenge["layout"]].append(accurate_words_per_minutes(challenge))

            layout = challenge['layout']
            if layout == Layouts.ArcType.value:
                layout_pit[layout].append((challenge_rot_travel(challenge), arctype_ideal(challenge['prompt'])))
            elif layout == Layouts.TiltType.value:
                layout_pit[layout].append((challenge_rot_travel(challenge), tilttype_ideal(challenge['prompt']),
                                           tilttype_ideal_tuple(challenge['prompt'])))

            layout_durations[layout].append(challenge['time']['duration'])

            if challenge['type'] == 'Blind':
                layout_blind_io[layout].append((challenge['prompt'], challenge['output']))

    merged = merge_trials(*trials.values())
    layout_posses = {e.value: extract_layout_positions(merged, e.value) for e in Layouts}

    header = ["Layout", "Avg X", "Std Dev X", "Avg Y", "Std Dev Y", "Avg Z", "Std Dev Z", "Avg WPM", "Std Dev WPM",
              "Avg aWPM", "Std Dev aWPM",
              # "Avg t", "Std Dev t",
              "Avg Travel", "Std Dev Travel", "Avg PIT", "Std Dev PIT"]
    rows = [header]
    for layout in (e.value for e in Layouts):
        row = [layout]
        if layout in layout_posses:
            posses = layout_posses[layout]
            xs = [v[0] for v in posses]
            ys = [v[1] for v in posses]  # swap y and z?
            zs = [v[2] for v in posses]
            row.append(np.mean(xs))
            row.append(np.std(xs))
            row.append(np.mean(ys))
            row.append(np.std(ys))
            row.append(np.mean(zs))
            row.append(np.std(zs))
        else:
            row.extend(('', '', '', '', '', ''))

        wpm_data = reject_outliers(np.array(layout_wpm[layout]))
        layout_wpm[layout] = wpm_data
        row.append(np.mean(wpm_data))
        row.append(np.std(wpm_data))

        awpm_data = reject_outliers(np.array(layout_awpm[layout]))
        layout_awpm[layout] = awpm_data
        row.append(np.mean(awpm_data))
        row.append(np.std(awpm_data))

        dur_data = reject_outliers(np.array(layout_durations[layout]))
        layout_durations[layout] = dur_data
        # row.append(np.mean(dur_data))
        # row.append(np.std(dur_data))

        if layout in layout_pit:
            actual_data = reject_outliers(np.array([t[0] for t in layout_pit[layout]]))
            layout_actual_travel[layout] = actual_data
            row.append(np.mean(actual_data))
            row.append(np.std(actual_data))

            ideal_data = reject_outliers(np.array([t[1] for t in layout_pit[layout]]))
            layout_ideal_travel[layout] = ideal_data

            pit_data = reject_outliers(np.array([100 * t[0] / t[1] for t in layout_pit[layout]]))
            layout_pit[layout] = pit_data

            row.append(np.mean(pit_data))
            row.append(np.std(pit_data))
        else:
            row.extend(('', '', '', ''))

        rows.append(row)

    return Data(rows=rows, layout_pit=layout_pit, layout_wpm=layout_wpm, layout_awpm=layout_awpm,
                layout_posses=layout_posses, layout_ideal_travel=layout_ideal_travel,
                layout_actual_travel=layout_actual_travel, layout_durations=layout_durations,
                layout_blind_io=layout_blind_io)


class Data:
    def __init__(self, **kwargs):
        self.rows = kwargs['rows']
        self.layout_pit = kwargs['layout_pit']
        self.layout_wpm = kwargs['layout_wpm']
        self.layout_awpm = kwargs['layout_awpm']
        self.layout_posses = kwargs['layout_posses']
        self.layout_actual_travel = kwargs['layout_actual_travel']
        self.layout_ideal_travel = kwargs['layout_ideal_travel']
        self.layout_durations = kwargs['layout_durations']
        self.layout_blind_io = kwargs['layout_blind_io']
        for k, v in kwargs.items():
            self.__dict__[k] = v


def csv_rows():
    return get_data().rows


def write_csv(truncate=8):
    with open('../../Results/extracted_data.csv', 'w', newline='') as csvfile:
        rows = [[(trunc((10**truncate) * x) / (10**truncate) if isinstance(x, float) else x) for x in row] for row in csv_rows()]
        csv.writer(csvfile).writerows(rows)


if __name__ == '__main__':
    data = get_data()
    make_point_cloud(data)
    # make_wpm_bars(data)
    # make_2d_point_cloud(data, [0, 2])
    # make_2d_point_cloud(data, [1, 2])
    make_pit_bars(data)
    make_error_pies(data)
    write_csv(truncate=5)
    # make_duration_lines(data)  # broken