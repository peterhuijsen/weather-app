import React, {FormEvent} from "react";
import {useRouter} from "next/router";
import {extractCookieAuthenticationData} from "@/src/services/token";
import Input from "@/components/Input";
import Button from "@/components/Button";
import RightArrowIcon from "@/components/icons/RightArrowIcon";
import SignOutIcon from "@/components/icons/SignOutIcon";
import {deleteCookie} from "cookies-next";
import Title from "@/components/general/Title";
import {verifyMfa} from "@/src/services/auth/otp";

type MfaProps = {
	uuid: string,
	token: string
}

export default function Mfa({uuid, token}: MfaProps) {
	const router = useRouter();

	async function mfaVerify(event: FormEvent<HTMLFormElement>) {
		event.preventDefault()

		try {
			await verifyMfa(
				event.currentTarget.code.value,
				uuid,
				token
			);

			await router.push("/");
		} catch (e) {
			console.log(e)
		}
	}

	async function logout() {
		deleteCookie("u");
		router.reload();
	}

	return (
		<>
			<div className={"flex flex-col p-8 w-[50%] bg-white rounded-xl shadow-lg"}>
				<div className={"flex flex-col gap-4"}>
					<Title>
						Multi-factor authentication
					</Title>

					<form onSubmit={mfaVerify} className={"flex flex-col gap-4"}>
						<Input
							type={"number"}
							name={"code"}
							title={"Code"}
							placeholder={"012345"}
							focus
							required
						/>

						<Button
							type={"submit"}
							style={"primary"}
							icon={<RightArrowIcon className={"text-white w-4 h-4"}/>}
							title={"Continue"}
						/>
					</form>
				</div>
			</div>
			<div className={"absolute top-8 right-8"}>
				<div className={"flex flex-col gap-4"}>
					<button onClick={() => logout()}
							className={"flex flex-col p-4 aspect-square bg-white rounded-full shadow-lg group"}>
						<SignOutIcon className={"text-neutral-900 w-6 h-6 group-hover:text-primary"}/>
					</button>
				</div>
			</div>
		</>
	)
}

export async function getServerSideProps({req}: any) {
	const [token, uuid] = extractCookieAuthenticationData(req.headers.cookie);

	try {
		if (uuid === undefined || token === undefined)
			return {
				redirect: {
					permanent: false,
					destination: "/login",
				}
			}

		return {props: {uuid, token}};
	} catch (e) {
		console.log(e)
		return {props: {}};
	}
}