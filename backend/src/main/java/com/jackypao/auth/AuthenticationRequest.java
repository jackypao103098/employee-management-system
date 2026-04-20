package com.jackypao.auth;

public record AuthenticationRequest(
        String username,
        String password
) {
}
