import {Passkey} from "@/src/models/auth/passkeys";
import {Otp} from "@/src/models/auth/otp";

export type UserUnauthorizedResponse = {
    message: string,
    url: string
}

export type User = {
    uuid: string,
    email: string,
    username: string,
    credentials: Credentials
}

export type Credentials = {
    mfa: boolean,
    hash: string,
    google: string,
    microsoft: string,
    otp: Otp,
    passkeys: Passkey[]
}

