import React, {Suspense, useState} from "react";
import {getUser} from '@/src/services/users';
import {extractCookieAuthenticationData} from "@/src/services/token";
import Navbar from "@/components/general/Navbar";
import Subtitle from "@/components/general/Subtitle";
import Title from "@/components/general/Title";
import {User, UserUnauthorizedResponse} from "@/src/models/user";
import WeatherPrediction from "@/components/WeatherPrediction";
import {getTemperature} from "@/src/services/temperature";

type HomeProps = {
	user: User
}

export default function Home({user}: HomeProps) {
	return (
		<>
			<div className={"flex flex-col p-8 bg-white rounded-xl shadow-lg"}>
				<div className={"flex flex-col gap-4"}>
					<Subtitle>
						Hello {user?.username}!
					</Subtitle>

					<Suspense fallback={
						<Title>
						It is ... degrees outside.
						</Title>
					}>
						<WeatherPrediction />
					</Suspense>

				</div>
			</div>
			<Navbar selectedMenu={"dashboard"}/>
		</>
	)
}

export async function getServerSideProps({req}: any) {
	const [token, uuid] = extractCookieAuthenticationData(req.headers.cookie);

	try {
		const isUserUnauthorizedResponse = (u: any): u is UserUnauthorizedResponse => u.url !== undefined;

		const user = await getUser(uuid, token);
		const isUnauthorizedResponse = isUserUnauthorizedResponse(user);

		if (isUnauthorizedResponse)
			return {
				redirect: {
					permanent: false,
					destination: "/mfa",
				}
			}

		return {props: {user}};

	} catch (e) {
		return {
			redirect: {
				permanent: false,
				destination: "/login",
			}
		}
	}
}