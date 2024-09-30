// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <map>
#include <thread>

std::map<short, short> weapon_map = {};
short current_hp;

DWORD WINAPI main_thread(LPVOID param) {
    uintptr_t base = (uintptr_t)GetModuleHandle(NULL);
    uintptr_t hp_pointer;
    uintptr_t current_weapon_pointer;
    uintptr_t current_mag_pointer;
    std::thread damage_thread(permanent_damage, hp_pointer);
    std::thread quick_reload_thread(no_quick_reload, current_weapon_pointer, current_mag_pointer);

    while (true) {
        
    }
    FreeLibraryAndExitThread((HMODULE)param, 0);
    damage_thread.join();
    quick_reload_thread.join();
    return 0;
}

void permanent_damage(uintptr_t hp_pointer_location) {
    //set max rations to 0
    //monitor current HP
    //if HP goes up, reset back down to last known value
}

void no_quick_reload(uintptr_t current_weapon_location, uintptr_t current_mag_count_location) {
    //when a weapon is equipped for the first time, initialize it
    //store dictionary of weapons/mag count
    //if weapon is re-equipped, restore to last known mag count
    short current_weapon;
    while (true) {
        if (weapon_map.find(current_weapon) == weapon_map.end()) {
            //add to list
        }
        else {
            short current_mag_count;
            weapon_map.find(current_weapon)->second = current_mag_count;
        }
    }
}

void no_pausing(uintptr_t pause_button_function, uintptr_t item_pause_function, uintptr_t weapon_pause_function) {
    //just disable all 3 sources of pausing
    //could disable codec pausing too, but that can cause crashes
}

void extended_guard_statuses() {
    //needs additional research
    //need to extend Alert
    //need to extend Evasion
    //need to extend Caution
}

void no_continues() {
    //needs additional research
    //can we just make it so the continue button doesnt appear?
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        CreateThread(0, 0, main_thread, hModule, 0, 0);
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

