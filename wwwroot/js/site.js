// BADEA Portal shared browser behaviour.

(() => {
    "use strict";

    // This guard is intentionally limited to the portal homepage.
    // Internal pages keep normal Back/Forward behaviour.
    function isPortalHome() {
        const normalizedPath =
            (window.location.pathname || "/")
                .replace(/\/+$/, "") || "/";

        return normalizedPath === "/" ||
            normalizedPath.toLowerCase() === "/home/index";
    }

    if (!isPortalHome()) {
        return;
    }

    const ROOT_STATE_KEY = "__badeaPortalRoot";
    const GUARD_STATE_KEY = "__badeaPortalBackGuard";

    function makeState(key) {
        const existing =
            history.state && typeof history.state === "object"
                ? history.state
                : {};

        return {
            ...existing,
            [key]: true
        };
    }

    function installBackGuard() {
        if (!isPortalHome()) {
            return;
        }

        const currentState =
            history.state && typeof history.state === "object"
                ? history.state
                : {};

        // Mark the real homepage entry. This does not navigate anywhere.
        if (!currentState[ROOT_STATE_KEY] &&
            !currentState[GUARD_STATE_KEY]) {
            history.replaceState(
                makeState(ROOT_STATE_KEY),
                "",
                window.location.href);
        }

        // Add one same-origin history entry in front of the authentication
        // history. A normal Back click lands on the homepage entry below,
        // where popstate immediately restores this guard entry.
        const stateAfterReplace =
            history.state && typeof history.state === "object"
                ? history.state
                : {};

        if (!stateAfterReplace[GUARD_STATE_KEY]) {
            history.pushState(
                makeState(GUARD_STATE_KEY),
                "",
                window.location.href);
        }
    }

    installBackGuard();

    window.addEventListener("popstate", () => {
        if (!isPortalHome()) {
            return;
        }

        // The user pressed Back while on the portal homepage.
        // Restore the guard instead of allowing the next Back step to expose
        // Microsoft's one-time OIDC/login history entries.
        history.pushState(
            makeState(GUARD_STATE_KEY),
            "",
            window.location.href);
    });

    // Safari and other browsers can restore the homepage from the
    // back/forward cache. Re-establish the guard if that happens.
    window.addEventListener("pageshow", (event) => {
        if (event.persisted) {
            installBackGuard();
        }
    });
})();
